# NCDO
Copyright (c) 2017 - 2018 Robin Herbots Licensed under the MIT license (http://opensource.org/licenses/mit-license.php)  
  
The NCDO is a .NET implementation of the <a href="https://github.com/CloudDataObject/CDO">CDO Specification</a> published by Progress Software Corporation. 
The NCDO is a free and open-source implementation that can be used in .NET. 

### Documentation
For more information see the <a href="https://documentation.progress.com/output/pdo">Progress Data Objects Guide and Reference.</a>

### Usage
Reference the NCDO nuget package in your project.

In your application the usage is simular as in the documentation of JSDO.

```
  var pdSession = new CDOSession(new Uri("http://<pas server url>"));
  pdSession.Login().Wait();
  pdSession.AddCatalog(new Uri("http://<catalog url>")).Wait();
            
  var cdo = new CDO("resource");
  var paramObj = new JsonObject
       {
          { "name", "name" },
       };
  
  var resp = cdo.Invoke("InvokeOperation", paramObj).Result;
```


### Remarks

NCDO is a wip and thus not feature complete.  
The current session object implementation is for anonymous authentication only.  
Other authentication models can be implemented by deriving from CDOSession and overriding the OnOpenRequest function.

As the project matures more features will become available.  
Any contributions (code, documentation) is also welcome. 
