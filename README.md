# NCDO
Copyright (c) 2017 - 2019 Robin Herbots Licensed under the MIT license (http://opensource.org/licenses/mit-license.php)  
  
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
- bearer_OnBehalf (Azure authentication onbehalf flow)

#### bearer_WIA
Windows Integrated Authentication needs to be enabled in AAD (Seamless single sign-on).

#### bearer_OnBehalf

The options UserAccessToken and UserName are of type Func<string>, allowing to dynamically call fot the current accesstoken and username.
```
  var pdSession = new CDOSession(new CDOSessionOptions() { 
        ServiceUri = new Uri("http://<pas server url>")
        AuthenticationModel = AuthenticationModel.Bearer_OnBehalf,
        Authority = "https://login.microsoftonline.com/<tenantId>",
        ClientId = <ClientId of the authenticated app>,
        ClientSecret = <ClientSecret of the authenticated app>,
        Audience = <ClientId of the target app/resource>,
        UserAccessToken = () => HttpContext.GetTokenAsync("id_token").Result, //function to retrieve the current accesstoken
        UserName = () => User.Identity.Name //function to retrieve the current username
  
  });
  pdSession.Login().Wait();
  pdSession.AddCatalog(new Uri("http://<catalog url>")).Wait();
            
  var cdo = new CDO("resource");
  var paramObj = new JsonObject
       {
          { "name", "name" },
       };
  
  var resp = cdo.Invoke("InvokeOperation", paramObj).Result;
```

###### ASP.NET Core / MVC

- appsettings.json

```
...
 "NCDOServiceClientOptions": {
    "ServiceUri": "http://<catalog url>",
    "AuthenticationModel": "Bearer_OnBehalf",
    "Authority": "https://login.microsoftonline.com/<tenant>",
    "ClientId": <clientId>,
    "ClientSecret": <clientSecret>,
    "Audience": <audience>
  },
...
```

- Startup

```
   public void ConfigureServices(IServiceCollection services)
   {
            ...
            services.Configure<NCDOServiceClientOptions>(Configuration.GetSection("NCDOServiceClientOptions"));
            var options = services.BuildServiceProvider().GetService<IOptions<NCDOServiceClientOptions>>().Value;
            services.AddSingleton(options);
            services.AddSingleton<NCDOServiceClient>();
            ...
   }
```

- Controller constructor
    
```
...
      _ncdoServiceClient.Options.UserName = () => User.Identity.Name;
      _ncdoServiceClient.Options.UserAccessToken = () => HttpContext.GetTokenAsync("id_token").Result;
...
```

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
    
- Custom capability for READ operation.  
    Only fetch the main table without the children.
    Set to false to not include the relations/childs
    
### Remarks
    
Any contributions (code, documentation) are welcome. 
