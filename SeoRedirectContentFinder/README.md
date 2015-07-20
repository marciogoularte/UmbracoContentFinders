# SeoRedirectContentFinder
Adds HTTP redirect support, e.g. for updating search engine indexes after site migrations.

## Getting started:
### 1.	Add Web.config appSetting
Add the following node to your Web.config <appSettings> section.
The path will be mapped relative to the website root. IIS virtual folders can be used by starting with a tilde (~) character.

Example:
```xml
<add key="SeoRedirectContentFinder.RedirectFilePath" value="~/seo/SeoRedirect.config" />
```

### 2. Create redirect config file
Create a .config file at the path as specified in step one, using the following structure.
```xml
<?xml version="1.0" encoding="utf-8" ?>
<seo>
 <domain name="www.mydomain.com">
  <redirect from="/about" to="/about-us" status="301" />
  <redirect from="/contact" to="/get-in-touch" status="301" />
 </domain>
</seo>
```

Multiple domain nodes can be added.
Make sure first testing your permanent (301) redirects without specifying the 'status' attribute.
This will result in a 302 redirect, which does not cause unintended client side caching of the redirect url.
When your redirects are working ok, set all permanent redirect status attributes to 301.