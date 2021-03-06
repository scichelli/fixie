﻿using System.Linq;
using Fixie.ConsoleRunner;
using Should;

namespace Fixie.Tests.ConsoleRunner
{
    public class CommandLineParserTests
    {
        public void DemandsAtLeastOneAssemblyPath()
        {
            var parser = new CommandLineParser();
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Missing required test assembly path(s).");
        }

        public void ParsesAssemblyPathsList()
        {
            var parser = new CommandLineParser("foo.dll", "bar.dll");
            parser.AssemblyPaths.ShouldEqual("foo.dll", "bar.dll");
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesNUnitXmlOutputFile()
        {
            var parser = new CommandLineParser("assembly.dll", "--NUnitXml", "TestResult.xml");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Keys.ShouldEqual("NUnitXml");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesXUnitXmlOutputFile()
        {
            var parser = new CommandLineParser("assembly.dll", "--XUnitXml", "TestResult.xml");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Keys.ShouldEqual("XUnitXml");
            parser.Options["XUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesTeamCityOutputFlag()
        {
            var parser = new CommandLineParser("assembly.dll", "--TeamCity", "off");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Keys.ShouldEqual("TeamCity");
            parser.Options["TeamCity"].ShouldEqual("off");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesCustomParameters()
        {
            var parser = new CommandLineParser("assembly.dll", "--parameter", "key=value", "--parameter", "otherKey=valueContaining=EqualSign");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Keys.ShouldEqual("key", "otherKey");
            parser.Options["key"].ShouldEqual("value");
            parser.Options["otherKey"].ShouldEqual("valueContaining=EqualSign");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void CustomParametersBehaveLikeSwitchesWhenNoEqualSignIsSpecified()
        {
            var parser = new CommandLineParser("assembly.dll", "--parameter", "switch");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Keys.ShouldEqual("switch");
            parser.Options["switch"].ShouldEqual("on");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void DemandsThatCustomParameterKeysCannotBeEmpty()
        {
            var parser = new CommandLineParser("assembly.dll", "--parameter", "=value");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Custom parameter =value is missing its required key.");
        }

        public void DemandsThatAllOptionsBeRecognized()
        {
            var parser = new CommandLineParser("assembly.dll", "--typo", "value");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --typo is not recognized.");
        }

        public void DemandsThatOptionsHaveExplicitValues()
        {
            var parser = new CommandLineParser("--NUnitXml", "TestResult.xml", "--XUnitXml");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Keys.ShouldEqual("NUnitXml");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --XUnitXml is missing its required value.");
        }

        public void DemandsThatOptionValuesCannotLookLikeKeys()
        {
            var parser = new CommandLineParser("--NUnitXml", "--anotherKey");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --NUnitXml is missing its required value.");
        }

        public void ParsesAllCustomParameterValuesProvidedForEachCustomKeyKey()
        {
            var parser = new CommandLineParser("assembly.dll", "--parameter", "a=1", "--parameter", "b=2", "--parameter", "a=3");

            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Keys.OrderBy(x => x).ShouldEqual("a", "b");
            parser.Options["a"].ShouldEqual("1", "3");
            parser.Options["b"].ShouldEqual("2");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesOptionNamesCaseInsensitive()
        {
            var parser = new CommandLineParser("assembly.dll", "--nUnItXmL", "NUnit.xml", "--XuNiTxMl", "XUnit.xml", "--tEaMcItY", "off", "--pArAmEtEr", "key=value");

            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Keys.OrderBy(x => x).ShouldEqual("key", "NUnitXml", "TeamCity", "XUnitXml");
            parser.Options["key"].ShouldEqual("value");
            parser.Options["NUnitXml"].ShouldEqual("NUnit.xml");
            parser.Options["TeamCity"].ShouldEqual("off");
            parser.Options["XUnitXml"].ShouldEqual("XUnit.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesAssemblyPathsMixedWithOptionsAndCustomParameters()
        {
            var parser = new CommandLineParser("a.dll", "--parameter", "include=CategoryA", "b.dll", "--NUnitXml", "TestResult.xml", "--parameter", "c.dll", "d.dll", "--parameter", "include=CategoryB", "--parameter", "mode=integration", "e.dll");

            parser.AssemblyPaths.ShouldEqual("a.dll", "b.dll", "d.dll", "e.dll");

            parser.Options.Keys.OrderBy(x => x).ShouldEqual("c.dll", "include", "mode", "NUnitXml");
            parser.Options["c.dll"].ShouldEqual("on");
            parser.Options["include"].ShouldEqual("CategoryA", "CategoryB");
            parser.Options["mode"].ShouldEqual("integration");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.Options["nonexistent"].ShouldBeEmpty();
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }
    }
}