# TO DO #

## ArasSync tool ##

* Support extraction/merging with template code in method-config.xml so that we can code/compile locally with
  full IDE support. (The current .cs fragments give a lot of intellisense errors.)

## Aras utility classes ##

* Investigate use of NLog instead of homegrown logging framework.
  
## Test architecture ##

* Rewrite all tests to use NUnit instead of MS Test Framework.
* Rename category "IntegrationTest" => "StagingTest" (as it requires code to run on server).
* Categorize tests (that communicate with Aras server as "IntegrationTest" (i.e. almost all).

## Documentation ##

* Use DocFX to generate both API and ad-hoc documentation.
