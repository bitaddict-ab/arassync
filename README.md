# README #

This repo contains ARAS Innovator PLM customizations for Consilum Marine & Safety AB.

It uses common code from an upstream repo shared by several Bit Addict customers,
which is placed in the BitAddict.Aras namespace.


### What is this repository for? ###

#### Quick summary

Provides company-specific extensions to the Aras Innovator PLM.
Also provides a base kit to simplify implementing, testing and managing Aras extensions.

Aras extensions are written as C# DLLs, server-methods (C#/VB), javascript and HTML,
as well as database objects such as custom properties, fields, actions et.al.

The scripts and tools found here allows developers to import, export, implement,
build, test and deploy Aras Extensions with relative ease.

### Details

This repo is available at https://bitbucket.org/bitaddict/consilium-aras-plm .

The upstream repo is found at https://bitbucket.org/bitaddict/bit-addict-aras-plm .


### How do I get set up? ###

#### Summary of set up

 * Start VisualStudio.cmd
 * Optional: enter your Aras login/password to allow unit tests and import/export, just press enter to skip.
 * Code and run tests from Visual Studio.
 * Use 'arassync' from command line to build release dll's and sync db data between repo and an Aras DB.

You can also run just CommandPrompt.cmd to avoid starting Visual Studio.

#### Command-line tools

Available commands for ArasSync.exe are:

Global commands (run anywhere in repo):

    ListDB      - List Aras instances specified in arasdb.json file(s).
    ForAll      - Runs an 'arassync' command in every feature directory.

    Login       - Asks user for Aras name/password and stores encrypted on disk.
    Logout      - Removes login information previously stored on disk.
    LoginStatus - Returns 0 (ok) if user is logged in or 1 if not.

    help <name> - For help with a specific command.

Feature commands (run in project subfolder):

    CopyDLL     - Builds and copies DLLs for the current feature to the Aras Web Server bin folder.
    Import      - Imports the current directory's feature from local disc into an Aras database.
    Export      - Exports the current directory's feature from Aras database to disk.

    ExtractAll  - Extract all xml-tags in manifest from AML and writes to on-disk code file.
    ExtractAML  - Extract xml-tag from AML and writes to on-disk code file.

    MergeAll    - Merges all xml-tags in manifest from files into AML file.
    MergeAML    - Updates an xml-tag in AML from on-disk code file.

#### Dependencies

 * Visual Studio 2015
 * Git
 * Optional: Jetbrains ReSharper VS addin (for formatting & style help)
 * Optional: EditorConfig VS addin (for consistent indentation)  - http://editorconfig.org/

#### Database configuration

arasdb/arasdb-json.local defines Aras instances (Web browser HTTP URL, IIS server folder and SQL database name)

A single instance is defined to be used as the developer instance,
which is were unit tests fetch data and integeration test methods are executed.

Create arasdb-local.json if you have user-specific Aras databases/instances to test with.

#### How to run tests

 * Start VisualStudio.cmd.
 * Enter login/password if required.
 * Use the built-in test runner in VS2015,
   i.e. right click a test-project/-class/-method and select 'run unit test(s)'.

#### Deployment instructions

 * Run 'arassync copydll -db=[DBID]' to build and copy DLLs to web application
 * Update C:/Program Files (x86)/[aras]/server/method-config.xml and add copied DLLs (and using BitAddict.Aras)
 * Run 'arassync import -db=[DBID]' to update database objects

 Use 'arassync forall \<command> \<args>' to run a command for all features (i.e subdirs with 'amlsync.json') in repo.

### Contribution guidelines ###

#### Writing tests

Add unit tests and/or integration tests using MS Unittest Framework.

Unit Test classes should inherit ArasExtensions.ArasTestCaseBase and reimplement ClassInitialize/ClassCleanup.

#### Code review

Use EditorConfig and Resharper to get a consistent formatting.

#### Other guidelines

To get consistent logging and automated error handling, server methods should
inherit ArasMethod and use the custom ApplyXXX methods.

### Who do I talk to? ###

* Marcus Sonestedt (marcus.lindblom.sonestedt@bitaddict.se)
* Per Olsson (per.olsson@consilium.se)

