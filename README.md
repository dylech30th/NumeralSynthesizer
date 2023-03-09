# NumeralSynthesizer
Get f**ked by *les numbres en Francais*?

### How to use?

```
NumeralSynthesizer.exe -key <your_azure_speech_service_key> -region <synthesizer_service_region> -config <config_file_path>
```

Whether the config file actually presents is irrelevant, the program creates one for you automatically.
For example:

```
NumeralSynthesizer.exe -key xxxxxxxxxxxxxxxxxxx -region eastasia -config config.json // it is OK that the config.json does not exist.
```

### Hey the code organizes like shit!
**Yes.** Still you can find the parser from *numerals* to *numbers* and vice-versa in *Parser.cs*, and the interactive Q&A part in *SynthesizerServices.cs*.

### Why I wrote this?
Because I'm boring.

### It has bugs!
Who cares.

### Can't read French!
So am I.
