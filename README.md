# ArasSync

This repo contains tools and base classes for Aras Innovator PLM extensions.

Code is developed & maintained by Bit Addict AB, but copyright belongs to:

* Consilium Marine & Safety AB
* CPAC Systems AB

This code has been developed, paid for and generously licensed under the MIT license by the above companies
See COPYING.TXT for full license/copyright information.

## What is this repository for?

### Quick summary

Provides a base kit to simplify implementing, testing and managing Aras extensions.

Aras extensions are written as a combination C# DLLs, server-methods (C#/VB), JavaScript and HTML,
as well as database objects such as custom properties, fields, actions et.al.

The scripts and tools found here allows developers to import, export, implement,
build, test and deploy Aras Extensions with relative ease.

In particular, features can be developed, tested and deployed on different environments/databased,
like development, test and production separately.

## How do I get set up?

### Summary of set up

 * Start VisualStudio.cmd
 * Optional: enter your Aras login/password to allow unit tests and import/export, just press enter to skip.
 * Code and run tests from Visual Studio.
 * Use 'arassync' from command line to build release DLLs and sync db data between repo and an Aras DB.

You can also run just CommandPrompt.cmd to avoid starting Visual Studio.

## Command-line tool

The available commands for ArasSync.exe are: (run 'arassync help' to see:)

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
    DeployDLL   - Builds and copies DLLs & docs for the current feature to the Aras Web Server bin folder
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

### Database configuration

arasdb.json & arasdb-local.json defines Aras instances (Web browser HTTP URL, IIS server folder and SQL database name)

A single instance is defined to be used as the developer instance,
which is were unit tests fetch data and integration test methods are executed.

Create arasdb-local.json if you have user-specific Aras databases/instances to test with, such as on your own computer.

An aras-db.json typically looks like this:

* It can/should changed in your local company's fork.)
* The example server shares the folder Aras from it's local C:\\Program Files (x86)\\Aras 

(

```json
{
  "DevelopmentInstance": "LOCAL",
  "Instances": [
    {
      "Id": "LOCAL",
      "Url": "http://localhost/innovatorServer",
      "DbName": "InnovatorSample",
      "BinFolder": "C:\\Program Files (x86)\\Aras\\Innovator"
    },
    {
      "Id": "PRODUCTION",
      "Url": "http://aras.mycompany.local/ArasProd/",
      "DbName": "ArasProduction",
      "BinFolder": "\\aras.mycompany.local\\Aras\\ArasProd\\Innovator" 
    },
    {
      "Id": "TEST",
      "Url": "http://aras.mycompany.local/ArasTest/",
      "DbName": "ArasTest",
      "BinFolder": "\\aras.mycompany.local\\Aras\\ArsasTest\\Innovator"
    }
  ],
  "DeployDll": {
    "Extensions": [ ".dll", ".pdb" ],
    "Excludes": [
      "Aras.Diagnostics",
      "Aras.Net",
      "IOM",
      "InnovatorCore",
      "Test",
      "UnitTestFramework",
      "JetBrains.Annotations",
      "nunit.framework"
    ]
  }
}
```

* The 'DevelopmentInstance' section selects which Aras instance will be used for unit-tests.
* The 'Instances' section is a list and is used to select where to push/pull data from. (db/AML connection, server directories, etc-)
* The 'DeployDll' section defines which extensions to copy from the .csproj target directory and which pattens/files to ignore.

## ArasSync Development

### Dependencies

 * Aras Innovator PLM (SP9 DLLs included here, but feel free to replace with your own. The IOM interface doesn't change much).
 * Visual Studio 2017 / 2019
 * Git
 * Optional: Jetbrains ReSharper VS addin (for formatting & style help)
 * Optional: EditorConfig VS addin (for consistent indentation)  - http://editorconfig.org/

### How to run unit tests

 * Run 'arassync login' and enter login/password if required.
 * Run Scripts\UnitTests.cmd (though most talk to the server)

You can also run them from within Visual Studio by right-clicking a project and choose "Run Unit Tests", if it has any tests in it..

Tests are currently a mix of MSTest and NUnit framework, but we plan to migrate fully to NUnit.

## Aras Feature Development

An Aras 'feature' (in the context of ArasSync) is typically a C# project with some AML files, plus possibly some JS/CSS/HTML for the client.

It is contained inside a single director, with a layout similar to this (based on the BitAddict.Aras.ExternalUrlWidget):

### Typical directory structure


```
BitAddict.Aras.GetExternalUrl/
├── Aras/ (files to be merged into AML)
│   ├── Method_MyFeature_X.cs             - an Aras server method
│   ├── Method_MyFeature_Y.js             - an Aras client method
│   ├── Customer_GetExternalUrl.js        - client side java script that overrides some Aras code
│   ├── Customer_GetExternalUrl.html      - client side HTML that is dynamically loaded by feature 
│   ├── Form_MyFeature_Z.html             - a HTML field inside a form
│   └── ...
├── ArasExport (directory structure as exported by export utility)
│   ├── GetExternalUrl/                   - package contents)
│   │    └── Import/
│   │       └── Method/
│   │          ├── MyFeature_Method_X.aml
│   │          └── MyFeature_Method_Y.aml
│   ├──PLM/  
│   │    └── Import/
│   │       └── Form/
│   │           └── Part.aml
│   ├── imports-001.mf                   - package definition for MyFeature
│   └── imports-002.mf                   - package definition for PLM
├── amlsync.json                         - defines how ArasSync should deploy this feature
├── BitAddict.Aras.GetExternalUrl.csproj - how to compile the C# DLL
├── app.config                           - .NET configuration file for the C# DLL
├── GetExternalUrlMethod.cs              - a class in the C# DLL
└── ...                                  - further files in the C# project

### Import manifests

The import manifest files (imports-xxx.mf) are created when a Package Definition is exported, and contains
the name, the export folder and any dependencies.

When exporting a single package, it gets the name 'imports.mf' so remember add a suffix number
if you are modifying several packages, as you need want one manifest for every Aras package that
is modified by your feature.

``´xml
<imports>
    <!-- This is our package, it depends on the PLM package where Part is defined -->
    <package name="BitAddict.Aras.GetExternalUrl" path="GetExternalUrl\Import">
        <dependson name="com.aras.innovator.solution.PLM" />
    </package>
</imports>
```` 

### amlsync.json

The amlsync.json files defines various parts of an Aras feature and how they are deployed/merged
with the existing Aras code tree. Note that an Aras feature does not necessarily use every section
of the amlsync.json method, rather only those that are required.

AMLSync.json contains three main parts

 * AmlFragments
 * XmlFragments
 * ServerFiles

#### AmlFragments

AmlFragments specify one or more AML files (export/import) and for each file, it defines
nodes whose text content should be in sync with a raw file on disk.

This helps when the content is C#, javascript or HTML, since the extracted files are
cleaner and have the proper file-ending, which allows your editor to use syntax highlighting
and do error checking.

```json
"AmlFragments": [
    {
      "amlfile": "ArasExport\\GetExternalUrl\\Import\\Method\\BitAddict_GetExternalUrl.xml",
      "nodes": [
        {
          "file": "Aras\\Method_BitAddict_GetExternalUrl.cs",
          "xpath": "//method_code"
        }
      ]
    },
    {
    "amlfile": "ArasExport\\PLM\\Import\\Form\\Part.xml",
    "nodes": [
        {   
        "file": "Aras\\BitAddict_GetExternalUrl_Snackbar.html",
        "xpath": "//Item[@id='FF93C32EDF144F1BA3F5BCD3EC972BE6']/html_code"
        }
    ]
]
```

JSON parts:

* amlfile - the XML file to inspect
* nodes - list of xml nodes to exract
* nodes/file - file to place extracted text into
* nodes/xpath - xpath expression that selects the XML node

Arassync commands:

* `ExtractAll` and `MergeAll` moves data from/to all AML files defined in amlsync.json.
* `ExtractAML` and `MergeAML` moves data from/to a single AML file and their extracted parts.

#### XmlFragments

This merges local XML pieces (defined inline or in a file) into XML files on the Aras server.

Typical use cases are in the example below:

 * Reference new .NET assemblies in method-config.xml, i.e. your C# project and it's dependencies.
 * Reference new JS files in IncludeNamespaceConfig.xml, so that they are loaded when the user accesses Aras via web browser.
 
```json
"XmlFragments": [
    {
      "RemoteFile": "Innovator\\Server\\method-config.xml",
      "Nodes": [
        {
          "fragment": "<name>$(binpath)/BitAddict.Aras.dll</name>",
          "existencexpath": "//MethodConfig/ReferencedAssemblies/name[text()='$(binpath)/BitAddict.Aras.dll']",
          "additionxpath": "//MethodConfig/ReferencedAssemblies"
        },
        {
          "fragment": "<name>$(binpath)/BitAddict.Aras.ExternalUrlWidget.dll</name>",
          "existencexpath": "//MethodConfig/ReferencedAssemblies/name[text()='$(binpath)/BitAddict.Aras.ExternalUrlWidget.dll']",
          "additionxpath": "//MethodConfig/ReferencedAssemblies"
        }
      ]
    },
    {
      "RemoteFile": "Innovator\\Client\\javascript\\IncludeNamespaceConfig.xml",
      "Nodes": [
        {
          "fragment": "<file src=\"Customer\\BitAddict_GetExternalUrl.js\"/>",
          "existencexpath": "//class[@name='ScriptSet1']/dependencies/file[@src='Customer\\BitAddict_GetExternalUrl.js']",
          "additionxpath": "//class[@name='ScriptSet1']/dependencies"
        }
      ]
    }
],
```

* remoteFile - file in Aras Folder
* nodes - list of XML nodes to update in Remote File
* nodes/fragment - XML data to add
* nodes/file - XML file to add 
* nodes/existencexpath: - XPath query to check if add is necessary or not
* nodes/additionxpath: - XPath query that returns parent of XML fragment

Note: fragment & file are mutually exclusive. One and only one of the may be defined.

#### ServerFiles

This defines files that should be uploaded to the server as is.

```json
"ServerFiles": [
    {
      "Local": "Aras\\Customer_BitAddict_GetExternalUrl.js",
      "Remote": "Innovator\\Client\\Javascript\\Customer\\BitAddict_GetExternalUrl.js"
    },
    {
      "Local": "Aras\\BitAddict_GetExternalUrl_Snackbar.html",
      "Remote": "Innovator\\Client\\Customer\\BitAddict_GetExternalUrl_Snackbar.html"
    }
]
```

* Local - File's path inside your project
* Remote - File's path on server 

### Aras Feature Deployment 

#### Update local DB files from Aras

It is often most convenient to first create a package definition and the appropriate Aras objects
inside Aras first, then export them into your git repository and add contents (i.e. method bodies, etc).

Also, every time you have made changes directly on the server related to your feature,
this should be exported and commited to git, so that your git repo matches the data your server.

* Run `arassync import --db=[DBID]` to update database objects (AML files) from an Aras instance

This will not export the Aras PLM package, as it consoleupgrade.exe does not support partial export
like the GUI tool export.exe does.

**Tip:**

you can extract the data there and commit any manual changes to git, before you make local changes,
commit and then deploy them. It allows you to verify that the server files are as expected.

#### Full deploy

* Run `arassync deploy --db=[DBID]` to build and deploy all aspects of a feature to an Aras instance

**Warning**

This will overwrite any manual changes made. While Aras methods have history, for instance Aras forms do
not, so it is recommended to export first and inspect any changes to your git working directory, then
merge with your intended changes, before deploying.

#### Binary only deploy

* Run `arassync deploydll --db=[DBID]` to build and deploy only the C# part of the project, i.e. binaries.

#### Deploy (or other) for all features

 Use `arassync forall \<command> \<args>` to run a command for all features (i.e subdirectories with 'amlsync.json') in repo.

## Contributing

Help out by looking at the issues, fixing bugs and adding features, both to arassync and
separate useful aras features.

1. Fork it!
2. Create your feature branch: git checkout -b my-new-feature
3. Commit your changes: git commit -am 'Add some feature'
4. Push to the branch: git push origin my-new-feature
5. Submit a pull request

### Guidelines

Use EditorConfig and Resharper to get a consistent formatting.

To get good and consistent logging and automated error handling, server methods should
inherit ArasMethod and use the custom ApplyXXX methods.

### Tests

Add unit tests and/or integration tests using MS Unittest Framework or NUnit (the latter is preferred).

Test classes that talk to Aras should:

* inherit BitAddict.Aras.Test.ArasUnitTestBase and re-implement ClassInitialize/ClassCleanup.
* or, inherit BitAddict.Aras.Test.ArasNUnitTestBase and re-implement ClassInitialize/ClassCleanup tagged with [SetUp]/[TearDown].

Tests that require code on the server should be categorized as "IntegrationTests".

# Credits / Responsibles #

Bit Addict AB:

* Marcus Sonestedt (marcus.lindblom.sonestedt@bitaddict.se)
* Jimmy Börjesson (jimmy.borjesson@bitaddict.se)
* Daniel Jonsson (daniel.jonsson@bitaddict.se)

Consilium Marine & Safety AB:

* Per Olsson (per.olsson@consilium.se)

CPAC Systems AB:

* Victor Stensson (victor.stensson@cpacsystems.se)
* Erik Råberg (erik.raberg@cpacsystems.se)
