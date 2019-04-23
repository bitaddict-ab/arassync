# ArasSync #

This repo contains tools and base classes for Aras Innovator PLM extensions.

Code is developed & maintained by Bit Addict AB, but copyright belongs to:

* Consilum Marine & Safety AB
* CPAC Systems AB

This code has been developed, paid for and generously licensed under the MIT license by the above compaines
See COPYING.TXT for full license/copyright information.

### What is this repository for? ###

#### Quick summary

Provides a base kit to simplify implementing, testing and managing Aras extensions.

Aras extensions are written as a combination C# DLLs, server-methods (C#/VB), JavaScript and HTML,
as well as database objects such as custom properties, fields, actions et.al.

The scripts and tools found here allows developers to import, export, implement,
build, test and deploy Aras Extensions with relative ease.

In particular, features can be developed, tested and deployed on different environemnts/databased,
like dev, test and production separately.

### How do I get set up? ###

#### Summary of set up

 * Start VisualStudio.cmd
 * Optional: enter your Aras login/password to allow unit tests and import/export, just press enter to skip.
 * Code and run tests from Visual Studio.
 * Use 'arassync' from command line to build release dll's and sync db data between repo and an Aras DB.

You can also run just CommandPrompt.cmd to avoid starting Visual Studio.

#### Command-line tools

The available commands for ArasSync.exe are: (run 'arasync help' to see:)

Advanced available commands are:

    ExtractAll         - Extract all xml-tags specified in amlsync.json from AML and writes to on-disk code file
    ExtractAML         - Extract xml-tag from AML and writes to on-disk code file
    ForAll             - Runs an 'arassync' command in every feature directory
    MergeAll           - Merges all xml-tags specified in amlsync.json from files into AML file
    MergeAML           - Updates an xml-tag in AML from on-disk code file
    ReplaceServerItems - Update the project's AML files with delete actions for relationships that exist in the database but not locally.
    UploadDoc          - Generates and uploads documentation from C#-sources for the current feature to the Aras Web Server bin/htmldoc/<feature> folder

    help <name>        - For help with one of the above commands

Standard available commands are:

    About       - Shows full license/copyright notice
    CopyDLL     - Builds and copies DLLs & docs for the current feature to the Aras Web Server bin folder
    Deploy      - Deploy the current directory's feature into the Aras instance
    Export      - Exports the current directory's feature from Aras database to disk
    ImportAML   - Imports the current directory's AML from local disc into an Aras database
    ImportFiles - Upload the current feature's server files from local disc into the Aras installation directory
    ImportXml   - Imports the current directory's XML fragments from local disc into an Aras installation directory
    ListDB      - List Aras instances specified in arasdb.json file(s)
    Login       - Asks user for Aras name/password and stores encrypted on disk.
    LoginStatus - Returns 0 (ok) if user is logged in or 1 if not.
    Logout      - Removes login information previously stored on disk.
    RunAML      - Run an AML query on the database

    help <name> - For help with one of the above commands

#### Dependencies

 * Aras Innovator PLM (SP9 DLLs included here, but feel free to replace with your own. The IOM interface doesn't change much).
 * Visual Studio 2017 / 2019
 * Git
 * Optional: Jetbrains ReSharper VS addin (for formatting & style help)
 * Optional: EditorConfig VS addin (for consistent indentation)  - http://editorconfig.org/

#### Database configuration

arasdb.json & arasdb-local.json defines Aras instances (Web browser HTTP URL, IIS server folder and SQL database name)

A single instance is defined to be used as the developer instance,
which is were unit tests fetch data and integeration test methods are executed.

Create arasdb-local.json if you have user-specific Aras databases/instances to test with, such as on your own computer.

#### How to run tests

 * Run 'arassync login' and enter login/password if required.
 * Run Scripts\UnitTests.cmd (though most talk to the server)

You can also run them from within Visual Studio by right-clicking a project and choose "Run Unit Tests", if it has any tests in it..

Tests aare currently a mix of MSTest and NUnit framework, but we plan to migrate fully to NUnit.

#### Deployment instructions

 * Run 'arassync deploy --db=[DBID]' to build and copy DLLs to web application
 * Run 'arassync import --db=[DBID]' to update database objects

 Use 'arassync forall \<command> \<args>' to run a command for all features (i.e subdirs with 'amlsync.json') in repo.

### Contributing

TODO: Tell the community how they can contribute to your project.

1. Fork it!
2. Create your feature branch: git checkout -b my-new-feature
3. Commit your changes: git commit -am 'Add some feature'
4. Push to the branch: git push origin my-new-feature
5. Submit a pull request

#### Guidelines

Use EditorConfig and Resharper to get a consistent formatting.

To get good and consistent logging and automated error handling, server methods should
inherit ArasMethod and use the custom ApplyXXX methods.

#### Tests

Add unit tests and/or integration tests using MS Unittest Framework or NUnit (the latter is preferred).

Test classes that talk to Aras should:

* inherit BitAddict.Aras.Test.ArasUnitTestBase and reimplement ClassInitialize/ClassCleanup.
* or, inherit BitAddict.Aras.Test.ArasNUnitTestBase and reimplement ClassInitialize/ClassCleanup tagged with [SetUp]/[TearDown].

Tests that require code on the server should be categorized as "IntegrationTests".


### Credits / Responsibles ###

* Marcus Sonestedt (marcus.lindblom.sonestedt@bitaddict.se)
* Jimmy Börjesson (jimmy.borjesson@bitaddict.se)
* Daniel Jonsson (daniel.jonsson@bitaddict.se)

* Per Olsson (per.olsson@consilium.se)
* Victor Stensson (victor.stensson@cpacsystems.se)
