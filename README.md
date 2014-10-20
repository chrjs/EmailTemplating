EmailTemplating
===============

EmailTemplating is a .NET class that allows to easily manage your EMail templates.

Features:
- Allows to put placeholders in your email templates in the format: @MyDataObject.Property.Property@
- Supports format strings: @MyDataObject.Property.Property:format@
- Allows to use a master template to be shared to all your email templates.
- No ASP.NET/MVC required: only standard framework classes are referenced.

Usage:
```c#
  EmailTemplate et = new EmailTemplate();
  et.AddDataObject("MyData", new DataClass());
  File.WriteAllText("email-body.html", et.GetEmailBody("template1.html", "master.html"));
```
