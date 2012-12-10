smartconf
=========

Smart Configuration Management for C#

[Listed on NuGet!](http://nuget.org/packages/SmartConf/)

I recently ran into a number of compounding issues that prompted me to create this project.

* I need a configuration file to store settings. This, alone, is easily covered by
    C#'s standard library ConfigurationManager.
* I have a subset of settings that need to be configured per-host. This is 
    accomplished with [configSource](http://blog.andreloker.de/post/2008/06/Keep-your-config-clean-with-external-config-files.aspx),
    also a standard feature of the ConfigurationManager.
* **I need to write settings back to a file**. This is the crux of the issue. ConfigurationManager
    is readonly, but I need to be able to store a value back into the config file before my program
    closes (for example, to record the last time the application was run).

This configuration manager allows multiple sources to be merged, in order, into a
single, strongly typed configuration object that can be passed into a program.


Features
========

* By default, uses C#'s standard XML-object [serialization](http://msdn.microsoft.com/en-us/library/system.xml.serialization.xmlserializer.aspx),
    so defining a configuration class is no different than defining a standard class.
* Custom IConfigurationSources may be defined to allow settings to be composed from
    alternate sources like AppConfig and the command line.
* Automatically detects changes using reflection. No need to explicitly mark a property as
    modified.
* Default constructors can also be used to set values not overridden by the base or local
    settings file (see note below about "dynamic" default values).
* The configuration manager intelligently *unmerges* the config instance before saving,
    so only properties that differ from the default (constructor) value and base settings
    value are serialized to a file. This allows you to change default (constructor) values
    and reuse the same config files even if they rely on the default values.
* The configuration object is separate from the configuration manager. There is no need to
    rewrite existing code to understand the config manager if all it needs are the settings
    themselves. So long as the same settings instance is passed to everything, the
    configuration manager is able to track changes to the object.
* Since configuration objects are plain objects, you can define instance methods, custom
    getters/setters, and more.

Existing Sources
================

Usage
=====

BaseSettings.xml:

    <Config>
        <Name>Horace</Name>
        <Webpage>google.com</Webpage>
    </Config>

LocalSettings.xml:

    <Config>
      <Name>Randall</Name>
    </Config>

Config object:

    public class Config
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Webpage { get; set; }
    }

Test code:

    public void SetAge(Config config)
    {
        config.Age = 20;
    }

    var configManager = new ConfigurationManager<Config>("BaseSettings.xml", "LocalSettings.xml");
    Config config = configManager.Out;

    Console.WriteLine(config.Name);
    //> Randall

    Console.WriteLine(config.Age);  // Default int value is 0
    //> 0

    Console.WriteLine(config.Webpage);
    //> google.com

    SetAge(config);
    Console.WriteLine(config.Age);
    //> 20

    config.Name = "Tim The Enchanter";
    foreach(var prop in configManager.GetPropertyChangesByName())
    {
        Console.WriteLine(prop.Key + ": " + prop.Value);
    }
    //> Name: Tim The Enchanter
    //> Age: 20

TODO
====

* Implement a way to disable tracking on-demand for short periods of time.
* Implement a custom source for AppConfig/WebConfig files and the built-in
  ConfigurationManager.

Notes
=====

* Since the XmlSerializer automatically initializes the default constructor on serialization,
    constructors that use a "dynamic" default value such as DateTime.Now may not correctly
    track changes when relying on that default value (ie. if neither the base settings file
    nor the local settings file explicitly set a DateTime initialized with Now, the time
    difference between serialization will result in different "default" values in the two
    objects). This may inadvertently cause the "default" value to be written to the local
    settings file as the manager detects the values are different. It is recommended to set
    properties to a constant value in the constructor and provide a separate initialization
    method that overwrites the constant (if not already overwritten).
* Only *public properties* are tracked by the manager, due in part to limitations with
  reflection.