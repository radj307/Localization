# radj307.Localization

Easy to use localization libraries for C# & WPF.  
Loosely based on [CodingSeb.Localization](https://github.com/codingseb/Localization).  

> ### :warning: Note
> This is still pre-1.0 and breaking changes should be expected.

| Nuget                         | Version                                                                                                                                                               |
|-------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **radj307.Localization**      | [![NuGet Status](http://img.shields.io/nuget/v/radj307.Localization.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/radj307.Localization)           |
| **radj307.Localization.WPF**  | [![NuGet Status](http://img.shields.io/nuget/v/radj307.Localization.WPF.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/radj307.Localization.WPF)   |
| **radj307.Localization.Json** | [![NuGet Status](http://img.shields.io/nuget/v/radj307.Localization.Json.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/radj307.Localization.Json) |
| **radj307.Localization.Yaml** | [![NuGet Status](http://img.shields.io/nuget/v/radj307.Localization.Yaml.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/radj307.Localization.Yaml) |
| **radj307.Localization.Xml**  | [![NuGet Status](http://img.shields.io/nuget/v/radj307.Localization.Xml.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/radj307.Localization.Xml)   |

## Packages

The library's functionality is divided between packages, allowing you to only install what you need.

- **radj307.Localization**  
  Contains the core library, the `Loc` class, and the translation functionality.
- **radj307.Localization.WPF**  
  Contains the `Tr` markup extension and some binding converters.  
  The provided markup extension allows you to swap languages at runtime without any additional code.
- **radj307.Localization.Json**  
  Contains the default serializer/deserializer for JSON translation config files.
- **radj307.Localization.Yaml**  
  Contains the default serializer/deserializer for YAML translation config files.
- **radj307.Localization.Xml**  
  Contains the default serializer/deserializer for XML translation config files.

## Basic Usage

### C#

```csharp
using Localization;

Loc.Instance.CurrentLanguageName = "English";
Loc.Instance.Translate("MainWindow.Text"); //< "Hello World!"

Loc.Instance.CurrentLanguageName = "French";
Loc.Instance.Translate("MainWindow.Text"); //< "Salut tout le monde!"

// Loc.Tr is equivalent to Loc.Instance.Translate
Loc.Tr("This.Key.Does.Not.Exist", defaultText: "Default Text!"); //< "Default Text!"
```

### XAML

No `xmlns` declaration is required, the `Tr` markup extension is available globally.

```xaml
<!--  The text will update automatically when you change the current language.  -->
<TextBlock Text="{Tr 'MainWindow.Text'}" />
<TextBlock Text="{Tr 'MainWindow.Text', DefaultText='(Translation Not Provided)'}" />
<TextBlock Text="{Tr {Binding MyProperty},
                     FormatString='[1]: {0} {2}',
                     FormatArgs={MakeArray {Binding Header},
                                           '\\'}}" />
```

### Translation Config Files

Translation configs store the translated strings for your application. You can use any file
 format or syntax you wish by writing an `ITranslationLoader` implementation for it. Default
 loader implementations are provided for JSON & YAML documents.

While it is recommended to keep languages separate for maintainability, there is nothing stopping
 you from saving multiple languages in the same file.

#### JSON

Syntax used by the `JsonTranslationLoader` class:  

```json
{
  "MainWindow": {
    "Text": {
      "English": "Hello World!",
      "French": "Salut tout le monde!"
    }
  }
}
```

Alternative syntax used by the `JsonSingleTranslationLoader` class:

```json
{
  "$LanguageName": "English",

  "MainWindow": {
    "Text": "Hello World!"
  }
}
```
`JsonSingleTranslationLoader` only supports 1 language per file, but the syntax is much easier to write.

#### YAML

Syntax used by the `YamlTranslationLoader` class:  

```yaml
MainWindow:
  Text:
    English: "Hello World!"
    French: "Salut tout le monde!"
```
