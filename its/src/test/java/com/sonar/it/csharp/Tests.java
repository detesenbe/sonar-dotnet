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
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.locator.FileLocation;
import java.io.File;
import java.io.IOException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;
import javax.annotation.CheckForNull;
import javax.annotation.Nullable;
import org.apache.commons.io.FileUtils;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.rules.TemporaryFolder;
import org.junit.runner.RunWith;
import org.junit.runners.Suite;
import org.junit.runners.Suite.SuiteClasses;
import org.sonarqube.ws.Components;
import org.sonarqube.ws.Issues;
import org.sonarqube.ws.Measures;

import static org.sonarqube.ws.Hotspots.SearchWsResponse.Hotspot;

@RunWith(Suite.class)
@SuiteClasses({
  AnalysisWarningsTest.class,
  AutoGeneratedTest.class,
  CasingAppTest.class,
  CodeDuplicationTest.class,
  CognitiveComplexityTest.class,
  CoverageTest.class,
  EnsureAllTestsRunTest.class,
  ExternalIssuesTest.class,
  IncrementalAnalysisTest.class,
  IssuesOnMissingFilesTest.class,
  MetricsIncludeHeaderCommentTest.class,
  MetricsTest.class,
  MultiTargetAppTest.class,
  MultipleProjectsTest.class,
  NoSonarTest.class,
  NoSourcesTest.class,
  ProjectLevelDuplicationTest.class,
  QualityProfileExportTest.class,
  RuleParameterCustomizationTest.class,
  SharedFilesTest.class,
  TestProjectTest.class,
  UnitTestProjectTypeProbingTest.class,
  UnitTestResultsTest.class,
  WebConfigTest.class
})
public class Tests {

  @ClassRule
  public static final Orchestrator ORCHESTRATOR = TestUtils.prepareOrchestrator()
    .addPlugin(TestUtils.getPluginLocation("sonar-csharp-plugin")) // Do not add VB.NET here, use shared project instead
    .restoreProfileAtStartup(FileLocation.of("profiles/no_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/class_name.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/template_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/custom_parameters.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/custom_complexity.xml"))
    .build();

  public static Path projectDir(TemporaryFolder temp, String projectName) throws IOException {
    Path projectDir = Paths.get("projects").resolve(projectName);
    FileUtils.deleteDirectory(new File(temp.getRoot(), projectName));
    File newFolder = temp.newFolder(projectName);
    Path tmpProjectDir = Paths.get(newFolder.getCanonicalPath());
    FileUtils.copyDirectory(projectDir.toFile(), tmpProjectDir.toFile());
    return tmpProjectDir;
  }

  @BeforeClass
  public static void deleteLocalCache() {
    TestUtils.deleteLocalCache();
  }

  static BuildResult analyzeProject(TemporaryFolder temp, String projectName, @Nullable String profileKey, String... keyValues) throws IOException {
    Path projectDir = Tests.projectDir(temp, projectName);

    ScannerForMSBuild beginStep = TestUtils.createBeginStep(projectName, projectDir)
      .setProfile(profileKey)
      .setProperties(keyValues);

    ORCHESTRATOR.executeBuild(beginStep);

    TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Restore,Rebuild");

    return ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));
  }

  static Components.Component getComponent(String componentKey) {
    return TestUtils.getComponent(ORCHESTRATOR, componentKey);
  }

  @CheckForNull
  static Integer getMeasureAsInt(String componentKey, String metricKey) {
    return TestUtils.getMeasureAsInt(ORCHESTRATOR, componentKey, metricKey);
  }

  @CheckForNull
  static Measures.Measure getMeasure(String componentKey, String metricKey) {
    return TestUtils.getMeasure(ORCHESTRATOR, componentKey, metricKey);
  }

  static List<Issues.Issue> getIssues(String componentKey) {
    return TestUtils.getIssues(ORCHESTRATOR, componentKey);
  }

  static List<Hotspot> getHotspots(String projectKey) {
    return TestUtils.getHotspots(ORCHESTRATOR, projectKey);
  }
}
