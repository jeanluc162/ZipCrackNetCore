using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ZipCrackNetCore.Library;

namespace ZipCrackNetCore.Tests
{
    [TestClass]
    public class GeneratorTests
    {
        [TestMethod]
        public void abc_length_three()
        {
            String Charset = "abc";
            List<String> Expected = new List<String>() { "aaa", "aab", "aac", "aba", "abb", "abc", "aca", "acb","acc",
                                                         "baa", "bab", "bac", "bba", "bbb", "bbc", "bca", "bcb","bcc",
                                                         "caa", "cab", "cac", "cba", "cbb", "cbc", "cca", "ccb","ccc"};
            List<String> Actual = new List<string>();
            foreach (String combination in new Generator(Charset, 3)) Actual.Add(combination);

            CollectionAssert.AreEqual(Expected, Actual);
        }
    }
}
