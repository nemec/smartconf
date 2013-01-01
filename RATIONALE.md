There's a lot of talk about configuration, and rightfully so. It's a very
complex topic and no one configuration system cater to everyone's needs.
Most of the talk focuses on one very specific aspect of configuration,
file formats, while ignoring an equally important piece of the puzzle. Namely,
*where* do the settings come from and what do we do once we've loaded them
into our program via our favorite serializer?

While it's natural for different categories of settings to reside in different
locations (think logging settings vs. general application settings), all too
often it's necessary for the *same* settings to be defined in multiple
locations. Two situations come to mind:

* In an application installed on a user's machine, you may keep default
    machine-wide settings stored in a privilaged area while allowing users
    to customize and override any or all of those settings and store those
    changes in their own user folder, away from other users.
* In an environment where one application may be distributed on multiple
    machines, a file of sane default settings may be stored in source
    control and distributed to each machine, while using a separate file
    to selectively override those settings on each server (think development
    vs. production Connection Strings). This is a typical pattern seen in
    Django (Python) settings modules, where there's a "base" settings module
    that imports settings from "local.py" and "dev.py" if they exist.

Existing solutions get us some of the way there, but XML transforms can
only do so much. There are times when that local "configuration file" isn't
even a file but instead drawn from a SQL database or the command line. In
times like that, configuration aggregation must be done within the code itself.

Once all configuration sources are identified, how are the sources accessed?
If each source represents a different section or area of configuration, there
isn't much issue -- logging settings are always stored in the LoggingConfig
variable, and so on. But once sources start overlapping, you need to remember
which one to check first and where to go next if that value is "null" or some
other arbitrary default value. In my opinion, a good configuration manager
should take care of that for you: set the order once and *it* knows which
sources to fall back on when the first try comes up blank.

Not content to just consume configuration files, many applications require
that some or all of their settings be mutable and allow those modified
settings to be persisted across sessions, usually by writing the values back
to whatever source they were retrieved from. A good configuration manager
(implemented in a sufficiently advanced programming language) should be able
to detect those changes and *only* write back those settings that changed
to prevent unchanged settings from finding their way into configuration
files they did not originate from. For example, say config A defines a `Name`,
`Age`, and `Favorite Color` and is *read only*. Say config B defines a
different `Favorite Color` and overrides values in config A. If all three
properties are written back to config B, config A might as well not exist
anymore because config B will just override each of them, even if config A
later changes `Age` to be something different.

I've referred to a generic "configuration manager" earlier, but haven't yet
defined what one is.

* Given multiple configuration "sources" and a hierarchy of "most important"
    to "least important", this configuration manager should intelligently
    pick the most important source that defines a property and return that
    value when you ask for that property by name.
* A configuration manager should be able to write values back to a source
    (if it has the right permissions) and should only write the minimum
    set of properties needed to completely recreate the current state of
    the configuration, assuming that the same set of sources are used with
    the same hierarchy.
* It should aggregate the settings into an easily digestible source. This
    is less important in dynamically typed languages that allow more freedom
    in defining objects, but in statically typed languages it's encouraged
    that a configuration manager produce strongly typed objects so that
    the settings object can take advantage of IDE tools like refactoring.
* The application that uses the configuration output should know as little
    as possible about the details of how the output was generated as possible.
    To steal a .Netism, the ideal configuration manager should generate a
    [Plain Old CLR Object](http://en.wikipedia.org/wiki/Plain_Old_CLR_Object),
    in short an object that requires no more dependencies than what's
    already in use by the application. The configuration manager should be as
    decoupled as possible from the application.
* This is less of a requirement, but allowing validation of the resulting
    output is also desirable.