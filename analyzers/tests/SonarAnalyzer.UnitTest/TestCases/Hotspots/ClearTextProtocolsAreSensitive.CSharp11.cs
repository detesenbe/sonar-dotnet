﻿using System;

const string protocol1 = """http://""";
const string protocol2 = """https://""";
const string address = """foo.com""";
const string noncompliant = $"""{protocol1}{address}"""; // Noncompliant
const string compliant = $"""{protocol2}{address}""";

const string a = """http://foo.com"""; // Noncompliant {{Using http protocol is insecure. Use https instead.}}

var b = "http://foo.com"u8; // FN
var c = """http://foo.com"""u8; // FN
var d = """
    http://foo.com
    """u8; // FN