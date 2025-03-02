/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011-2023 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Path;
import java.util.stream.Collectors;
import java.util.List;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Duplications;
import org.sonarqube.ws.Issues;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.Assert.assertTrue;

public class IncrementalAnalysisTest {
  private static final String PROJECT = "IncrementalPRAnalysis";
  private static final String PULL_REQUEST_KEY = "42";

  @Rule
  public TemporaryFolder temp = TestUtils.createTempFolder();

  @Before
  public void init() {
    TestUtils.reset(ORCHESTRATOR);
  }

  @Test
  public void incrementalPrAnalysis_NoCache_FullAnalysisDone() throws IOException {
    Tests.analyzeProject(temp, PROJECT, null, "sonar.branch.name", "base-branch", "sonar.analysisCache.enabled", "false");
    Path projectDir = Tests.projectDir(temp, PROJECT);
    File withChangesPath = projectDir.resolve("IncrementalPRAnalysis\\WithChanges.cs").toFile();
    addIssue(withChangesPath);

    BeginAndEndStepResults results = executeAnalysisForPRBranch(PROJECT, projectDir, "");
    BuildResult beginStepResults = results.getBeginStepResult();
    BuildResult endStepResults = results.getEndStepResult();

    assertTrue(endStepResults.isSuccess());
    assertThat(beginStepResults.getLogs()).contains("Processing analysis cache");
    assertThat(beginStepResults.getLogs()).contains("Cache data is empty. A full analysis will be performed.");
    assertAllFilesWereAnalysed(endStepResults, projectDir);
    List<Issues.Issue> allIssues = TestUtils.getIssues(ORCHESTRATOR, PROJECT, PULL_REQUEST_KEY);
    assertThat(allIssues).hasSize(1);
    assertThat(allIssues.get(0).getRule()).isEqualTo("csharpsquid:S1134");
  }

  @Test
  public void incrementalPrAnalysis_cacheAvailableNoChanges_nothingReported() throws IOException {
    Tests.analyzeProject(temp, PROJECT, null, "sonar.branch.name", "base-branch");
    Path projectDir = Tests.projectDir(temp, PROJECT);

    BeginAndEndStepResults results = executeAnalysisForPRBranch(PROJECT, projectDir, "");
    BuildResult beginStepResults = results.getBeginStepResult();
    BuildResult endStepResults = results.getEndStepResult();

    assertTrue(endStepResults.isSuccess());
    assertCacheIsUsed(beginStepResults, PROJECT);
    assertThat(endStepResults.getLogs()).doesNotContain("Adding normal issue S1134");
    List<Issues.Issue> allIssues = TestUtils.getIssues(ORCHESTRATOR, PROJECT, PULL_REQUEST_KEY);
    assertThat(allIssues).isEmpty();
  }

  @Test
  public void incrementalPrAnalysis_cacheAvailableChangesDone_issuesReportedForChangedFiles() throws IOException {
    Tests.analyzeProject(temp, PROJECT, null, "sonar.branch.name", "base-branch");
    Path projectDir = Tests.projectDir(temp, PROJECT);
    File unchanged1Path = projectDir.resolve("IncrementalPRAnalysis\\Unchanged1.cs").toFile();
    File unchanged2Path = projectDir.resolve("IncrementalPRAnalysis\\Unchanged2.cs").toFile();
    File withChangesPath = projectDir.resolve("IncrementalPRAnalysis\\WithChanges.cs").toFile();
    addIssue(withChangesPath);
    File fileToBeAddedPath = projectDir.resolve("IncrementalPRAnalysis\\AddedFile.cs").toFile();
    createFileWithIssue(fileToBeAddedPath);

    BeginAndEndStepResults results = executeAnalysisForPRBranch(PROJECT, projectDir, "");
    BuildResult beginStepResults = results.getBeginStepResult();
    BuildResult endStepResults = results.getEndStepResult();

    assertTrue(endStepResults.isSuccess());
    assertCacheIsUsed(beginStepResults, PROJECT);
    assertThat(endStepResults.getLogs()).doesNotContain("Adding normal issue S1134: " + unchanged1Path);
    assertThat(endStepResults.getLogs()).doesNotContain("Adding normal issue S1134: " + unchanged2Path);
    assertThat(endStepResults.getLogs()).contains("Adding normal issue S1134: " + withChangesPath);
    assertThat(endStepResults.getLogs()).contains("Adding normal issue S1134: " + fileToBeAddedPath);
    List<Issues.Issue> fixMeIssues = TestUtils
        .getIssues(ORCHESTRATOR, PROJECT, PULL_REQUEST_KEY)
        .stream()
        .filter(x -> x.getRule().equals("csharpsquid:S1134"))
        .collect(Collectors.toList());
    assertThat(fixMeIssues).hasSize(2);
    assertThat(fixMeIssues.get(0).getRule()).isEqualTo("csharpsquid:S1134");
    assertThat(fixMeIssues.get(0).getComponent()).isEqualTo("IncrementalPRAnalysis:IncrementalPRAnalysis/AddedFile.cs");
    assertThat(fixMeIssues.get(1).getRule()).isEqualTo("csharpsquid:S1134");
    assertThat(fixMeIssues.get(1).getComponent()).isEqualTo("IncrementalPRAnalysis:IncrementalPRAnalysis/WithChanges.cs");
  }

  @Test
  public void incrementalPrAnalysis_cacheAvailableProjectBaseDirChanged_everythingIsReanalyzed() throws IOException {
    Tests.analyzeProject(temp, PROJECT, null, "sonar.branch.name", "base-branch");
    Path projectDir = Tests.projectDir(temp, PROJECT);
    File withChangesPath = projectDir.resolve("IncrementalPRAnalysis\\WithChanges.cs").toFile();
    addIssue(withChangesPath);

    BeginAndEndStepResults results = executeAnalysisForPRBranch(PROJECT, projectDir, PROJECT);
    BuildResult beginStepResults = results.getBeginStepResult();
    BuildResult endStepResults = results.getEndStepResult();

    assertTrue(endStepResults.isSuccess());
    assertCacheIsUsed(beginStepResults, PROJECT);
    assertAllFilesWereAnalysed(endStepResults, projectDir);
    List<Issues.Issue> fixMeIssues = TestUtils
      .getIssues(ORCHESTRATOR, PROJECT, PULL_REQUEST_KEY)
      .stream()
      .filter(x -> x.getRule().equals("csharpsquid:S1134"))
      .collect(Collectors.toList());
    assertThat(fixMeIssues).hasSize(3);
    assertThat(fixMeIssues.get(0).getRule()).isEqualTo("csharpsquid:S1134");
    assertThat(fixMeIssues.get(0).getComponent()).isEqualTo("IncrementalPRAnalysis:Unchanged1.cs");
    assertThat(fixMeIssues.get(1).getRule()).isEqualTo("csharpsquid:S1134");
    assertThat(fixMeIssues.get(1).getComponent()).isEqualTo("IncrementalPRAnalysis:Unchanged2.cs");
    assertThat(fixMeIssues.get(2).getRule()).isEqualTo("csharpsquid:S1134");
    assertThat(fixMeIssues.get(2).getComponent()).isEqualTo("IncrementalPRAnalysis:WithChanges.cs");
  }

  @Test
  public void incrementalPrAnalysis_cacheAvailableDuplicationIntroduced_duplicationReportedForChangedFile() throws IOException {
    String projectName = "IncrementalPRAnalysisDuplication";
    Tests.analyzeProject(temp, projectName, null, "sonar.branch.name", "base-branch");
    Path projectDir = Tests.projectDir(temp, projectName);
    File originalFile = projectDir.resolve("IncrementalPRAnalysisDuplication\\OriginalClass.cs").toFile();
    File duplicatedFile = projectDir.resolve("IncrementalPRAnalysisDuplication\\CopyClass.cs").toFile();
    createDuplicate(originalFile, duplicatedFile);

    BeginAndEndStepResults results = executeAnalysisForPRBranch(projectName, projectDir, "");
    BuildResult beginStepResults = results.getBeginStepResult();
    BuildResult endStepResults = results.getEndStepResult();

    assertTrue(endStepResults.isSuccess());
    assertCacheIsUsed(beginStepResults, projectName);
    List<Duplications.Duplication> duplications = TestUtils.getDuplication(
        ORCHESTRATOR,
        "IncrementalPRAnalysisDuplication:IncrementalPRAnalysisDuplication/CopyClass.cs",
        PULL_REQUEST_KEY)
      .getDuplicationsList();
    assertThat(duplications).isNotEmpty();
  }

  private static void assertAllFilesWereAnalysed(BuildResult endStepResults, Path projectDir) {
    File unchanged1Path = projectDir.resolve("IncrementalPRAnalysis\\Unchanged1.cs").toFile();
    File unchanged2Path = projectDir.resolve("IncrementalPRAnalysis\\Unchanged2.cs").toFile();
    File withChangesPath = projectDir.resolve("IncrementalPRAnalysis\\WithChanges.cs").toFile();
    assertThat(endStepResults.getLogs()).contains("Adding normal issue S1134: " + unchanged1Path);
    assertThat(endStepResults.getLogs()).contains("Adding normal issue S1134: " + unchanged2Path);
    assertThat(endStepResults.getLogs()).contains("Adding normal issue S1134: " + withChangesPath);
  }

  private static void assertCacheIsUsed(BuildResult beginStepResults, String project) {
    assertThat(beginStepResults.getLogs()).contains("Processing analysis cache");
    assertThat(beginStepResults.getLogs()).contains("Downloading cache. Project key: " + project + ", branch: base-branch.");
  }

  private void addIssue(File file) throws IOException {
    BufferedWriter writer = new BufferedWriter(new FileWriter(file, true));
    writer.append("// FIXME: S1134");
    writer.close();
  }

  private void createFileWithIssue(File file) throws IOException {
    createFileWithContent(file,
      "namespace IncrementalPRAnalysis\n" +
        "{\n" +
        "public class AddedFile\n" +
        "{\n" +
        "}\n" +
        "}// FIXME: S1134");
  }

  private void createDuplicate(File oldFile, File newFile) throws IOException {
    BufferedReader reader = new BufferedReader(new FileReader(oldFile));
    String content = "";
    String currentLine;
    while ((currentLine = reader.readLine()) != null) {
      content += currentLine + System.lineSeparator();
    }
    reader.close();
    content = content.replace("OriginalClass", "CopyClass");
    createFileWithContent(newFile, content);
  }

  private void createFileWithContent(File file, String content) throws IOException {
    BufferedWriter writer = new BufferedWriter(new FileWriter(file));
    writer.write(content);
    writer.close();
  }

  private BeginAndEndStepResults executeAnalysisForPRBranch(String project, Path projectDir, String subProjectName) {
    ScannerForMSBuild beginStep;
    if (subProjectName.isEmpty()) {
      beginStep = TestUtils.createBeginStep(project, projectDir);
    } else {
      beginStep = TestUtils.createBeginStep(project, projectDir, subProjectName);
    }

    BuildResult beginStepResults = ORCHESTRATOR.executeBuild(beginStep
      .setProperty("sonar.pullrequest.base", "base-branch")
      .setProperty("sonar.pullrequest.key", PULL_REQUEST_KEY)
      .setProperty("sonar.pullrequest.branch", "pull-request")
      .setProperty("sonar.verbose", "true"));
    TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Restore,Rebuild");
    BuildResult endStepResults = ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));

    return new BeginAndEndStepResults(beginStepResults, endStepResults);
  }

  private final class BeginAndEndStepResults {
    private final BuildResult beginStepResult;
    private final BuildResult endStepResult;

    public BeginAndEndStepResults(BuildResult beginStepResult, BuildResult endStepResult) {
      this.beginStepResult = beginStepResult;
      this.endStepResult = endStepResult;
    }

    public BuildResult getBeginStepResult() {
      return beginStepResult;
    }

    public BuildResult getEndStepResult() {
      return endStepResult;
    }
  }
}
