﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Selenium.Axe;

namespace CSharpSeleniumWebdriverSample.Test
{
    [TestClass]
    [DeploymentItem("samplePage.html")]
    [DeploymentItem("chromedriver.exe")]
    [DeploymentItem("geckodriver.exe")]
    [TestCategory("Integration")]
    public class SampleTest
    {
        private IWebDriver _webDriver;
        private WebDriverWait _wait;
        private const string MainElementSelector = "main";
        private const int TimeOutInSeconds = 20;

        [TestCleanup]
        public virtual void TearDown()
        {
            _webDriver?.Quit();
            _webDriver?.Dispose();
        }


        [TestMethod]
        [DataRow("Chrome")]
        [DataRow("Firefox")]
        public void RunScanOnGivenElement(string browser)
        {
            string samplePageURL = @"src\samplePage.html";
            string integrationTestTargetFile = Path.GetFullPath(samplePageURL);
            string integrationTestTargetUrl = new Uri(integrationTestTargetFile).AbsoluteUri;
            string elementSelector = "ul";
            int ExpectedNumberOfViolation = 3;

            this.InitDriver(browser);
            LoadTestPage(integrationTestTargetUrl);

            var mainElement = _wait.Until(drv => drv.FindElement(By.TagName(elementSelector)));

            AxeResult results = _webDriver.Analyze(mainElement);
            results.Violations.Should().HaveCount(ExpectedNumberOfViolation);
        }


        private void LoadTestPage(string integrationTestTargetUrl)
        {
            _webDriver.Navigate().GoToUrl(integrationTestTargetUrl);

            _wait.Until(drv => drv.FindElement(By.TagName(MainElementSelector)));
        }

        private void InitDriver(string browser)
        {
            switch (browser.ToUpper())
            {
                //In case using Chrome Web Driver
                case "CHROME":
                    // Check if Chrome web driver eniroment varibale already defined so that Chrome version and the webdriver are the same, other wise use Selenium.WebDriver.ChromeDriver to indtall it
                    var chromeDriverDirectory = Environment.GetEnvironmentVariable("ChromeWebDriver") ?? Environment.CurrentDirectory;
                    ChromeOptions options = new ChromeOptions
                    {
                        UnhandledPromptBehavior = UnhandledPromptBehavior.Accept,
                    };
                    options.AddArgument("no-sandbox");
                    options.AddArgument("--log-level=3");
                    options.AddArgument("--silent");

                    ChromeDriverService service = ChromeDriverService.CreateDefaultService(chromeDriverDirectory);
                    service.SuppressInitialDiagnosticInformation = true;
                    _webDriver = new ChromeDriver(chromeDriverDirectory, options);

                    break;
                // Incase Using Firefox
                case "FIREFOX":
                    // Check if Gecko web driver eniroment varibale already defined so that Firefox version and the webdriver are the same, other wise use Selenium.WebDriver.GeckoDriver to indtall it
                    var geckoDriverDirectory = Environment.GetEnvironmentVariable("GeckoWebDriver") ?? Environment.CurrentDirectory;
                    _webDriver = new FirefoxDriver(geckoDriverDirectory);
                    break;

                default:
                    throw new ArgumentException($"Remote browser type '{browser}' is not supported");

            }

            _wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(TimeOutInSeconds));
            _webDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(TimeOutInSeconds);
            _webDriver.Manage().Window.Maximize();
        }
    }
}