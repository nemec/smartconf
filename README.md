smartconf
=========

Smart Configuration Management for C#

[Listed on NuGet!](http://nuget.org/packages/SmartConf/)

See RATIONALE.md for a more complete explanation, but suffice to say that
existing configuration systems (specifically in C#) are missing key
functionality that I end up needing at one point or another.

Specifically:

* I need a configuration file to store settings. This, alone, is easily covered by
    C#'s standard library ConfigurationManager.
* I have a subset of settings that need to be configured per-host. This is 
    accomplished with [configSource](http://blog.andreloker.de/post/2008/06/Keep-your-config-clean-with-external-config-files.aspx),
    also a standard feature of the ConfigurationManager, but it only
    works for merging XML configurations into other XML configurations.
* I'd like whatever configuration settings I deal with to be strongly
  typed since Visual Studio's refactoring tools are awesome.
* I need to write settings back to a file. And it would be nice if it
  didn't automatically write out *every* setting in the configuration object
  or require me to manually track each and every setting I change. 

This configuration manager allows multiple sources to be merged, in order, into a
single, strongly typed configuration object that can be passed into a program.


Features
========

* By default, uses C#'s standard XML-object [serialization](http://msdn.microsoft.com/en-us/library/system.xml.serialization.xmlserializer.aspx),
    so defining a configuration class is no different than defining a standard
    class.
* Custom IConfigurationSources may be defined to allow settings to be composed
    from alternate sources like AppConfig and the command line.
* Automatically detects changes using reflection. No need to explicitly mark a
    property as modified.
* Default constructors can also be used to set values not overridden by the
    base or local settings file (see note below about "dynamic"
    default values).
* The configuration manager intelligently *unmerges* the config instance
    before saving, so only properties that differ from the default
    (constructor) value and base settings value are serialized to a file.
    This allows you to change default (constructor) values and reuse the same
    config files even if they rely on the default values.
* The configuration object is separate from the configuration manager. There
    is no need to rewrite existing code to understand the config manager
    if all it needs are the settings themselves. So long as the same settings
    instance is passed to everything, the configuration manager is able to
    track changes to the object.
* Since configuration objects are plain objects, you can define instance
    methods, custom getters/setters, and more.
* Validation can be performed on the final configuration object to ensure that
    all of its values are valid.
* The configuration manager can mark a property that should be always
    serialized or never serialized (`ConfigManager.AlwaysSerialize` and
    `ConfigManager.NeverSerialize` respectively). A property marked as
    `AlwaysSerialize` will be serialized regardless of whether or not it
    changed. A property marked as `NeverSerialize` will never be serialized,
    even if it's changed. The latter is useful for properties like passwords. 
    The password may be stored in a protected configuration file that's only 
    visible to the ConfigurationManager, but the source where changed 
    properties are written may be world-readable. This way, changes to the
    password within the application are not persisted.

Existing Sources
================
Check out the [wiki](https://github.com/nemec/smartconf/wiki/Existing-Sources)
for a list.

Usage
=====

BaseSettings.xml:

    <Config>
        <Name>Horace</Name>
        <Occupation>Web Developer</Occupation>
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
        public string Occupation { get; set; }

        public Config()
        {
            Occupation = "Unemployed";
        }
    }

Test code:

    public void SetAge(Config config)
    {
        config.Age = 20;
    }

    var configManager = new ConfigurationManager<Config>(
        "BaseSettings.xml", "LocalSettings.xml");
    Config config = configManager.Out;

    Console.WriteLine(config.Name);
    //> Randall

    Console.WriteLine(config.Age);  // Default int value is 0
    //> 0

    Console.WriteLine(config.Occupation);
    //> Web Developer

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

Validation
==========

Occasionally, it may be necessary to perform validation on the resulting
configuration object. If an IValidator is passed to the ConfigurationManager
constructor, the final configuration will be tested to ensure it
validates. If validation fails, a ValidationException will be thrown.

To aid in simple validation, a RuleBasedValidator is built in. Boolean
rules (functions that return True if valid and False if not) may be added
and more complex delegates that throw ValidationExceptions to indicate
failure may also be added prior to constructing the ConfigurationManager.
If your needs are more complicated than this, custom IValidators may
be created.

Note that each source will not be tested individually -- since
a source is not required to fill in every single property, there is not
much sense in testing them on their own.

Example code:

    var validator = new Validation.RuleBasedValidator<Config>();
    
    // Example of a BooleanRule
    validator.AddRule(c => c.Name != null, "You must have a name!");

    // Example of a ComplexRule
    validator.AddRule(c => {
        if(c.Age < 16 && c.Occupation != "Unemployed")
        {
            throw new ValidationException("Child labor is illegal.");
        }
    }

    var configManager = new ConfigurationManager<Config>(
        validator, "BaseSettings.xml", "LocalSettings.xml");

    // [...snip...]


TODO
====

* Implement a way to disable tracking on-demand for short periods of time.
* Implement a custom source for AppConfig/WebConfig files and the built-in
  ConfigurationManager.

Notes
=====

* Since the XmlSerializer automatically initializes the default constructor
    on serialization, constructors that use a "dynamic" default value such
    as DateTime.Now may not correctly track changes when relying on that
    default value (ie. if neither the base settings file nor the local
    settings file explicitly set a DateTime initialized with Now, the time
    difference between serialization will result in different "default"
    values in the two objects). This may inadvertently cause the "default"
    value to be written to the local settings file as the manager detects
    the values are different. It is recommended to set properties to a
    constant value in the constructor and provide a separate initialization
    method that overwrites the constant (if not already overwritten).
* Only *public properties* are tracked by the manager, due in part to
  limitations with reflection.