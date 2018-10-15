# NCDO
Copyright (c) 2017 - 2018 Robin Herbots Licensed under the MIT license (http://opensource.org/licenses/mit-license.php)  
  
[![donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LXZNPVLB4P7GU)
  
 
The NCDO is a .NET implementation of the <a href="https://github.com/CloudDataObject/CDO">CDO Specification</a> published by Progress Software Corporation. 
The NCDO is a free and open-source implementation that can be used in .NET. 

### Documentation
For more information see the <a href="https://documentation.progress.com/output/pdo">Progress Data Objects Guide and Reference.</a>

### Usage
Reference the NCDO nuget package in your project.

In your application the usage is simular as in the documentation of JSDO.

```
  var pdSession = new CDOSession(new CDOSessionOptions() { ServiceUri = new Uri("http://<pas server url>")});
  pdSession.Login().Wait();
  pdSession.AddCatalog(new Uri("http://<catalog url>")).Wait();
            
  var cdo = new CDO("resource");
  var paramObj = new JsonObject
       {
          { "name", "name" },
       };
  
  var resp = cdo.Invoke("InvokeOperation", paramObj).Result;
```

### Authentication

The Authentication model is specified in the CDOSessionOptions and passed in the constructor of a CDOSession.  

Supported protocols:
- anonymous
- basic
- bearer
- bearer_WIA (Azure authentication through Windows Integrated Authentication)

### Remarks

Any contributions (code, documentation) is also welcome. 

#### bearer_WIA
Windows Integrated Authentication needs to be enabled in AAD (Seamless single sign-on).

#### Additions to api spec

- ICloudDataObject.Get

        /// <summary>
        ///     Searches for a record in a table referenced in CDO memory
        ///     and returns a reference to a dataset with all related data from the record if found. If no record
        ///     is found, it returns null.
        /// </summary>
        /// <returns></returns>
        Task<D> Get(Expression<Func<R, bool>> filter);

- CDOSession.ChallengeToken
    Returns the authentication token for the given authentication model