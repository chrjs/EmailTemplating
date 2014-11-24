using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace EmailTemplating
{
  internal class Placeholder
  {
    public Placeholder(string placeholder)
    {
      this.PlaceholderString = placeholder;
      placeholder = placeholder.Replace("<%", "").Replace("%>", "");

      int idx = placeholder.IndexOf(":");
      if (idx > 0)
      {
        this.Path = placeholder.Substring(0, idx);
        this.Format = placeholder.Substring(idx + 1);
      }
      else
      {
        this.Path = placeholder;
      }

      this.Fields = this.Path.Split(".".ToCharArray());
      this.RootObjectKey = this.Fields[0];
    }

    public string RootObjectKey { get; private set; }
    public string Path { get; private set; }
    public string Format { get; private set; }
    public string[] Fields { get; private set; }
    public string PlaceholderString { get; private set; }
  }

  public class EmailTemplate
  {
    public EmailTemplate()
    {
    }

    private Dictionary<string, object> placeholders
    {
      get;
      set;
    }

    public string GetEmailBody(string templateFilePath)
    {
      return this.GetEmailBody(templateFilePath, null, null);
    }

    public string GetEmailBody(string templateFilePath, Encoding encoding)
    {
      return this.GetEmailBody(templateFilePath, null, encoding);
    }

    public string GetEmailBody(string templateFilePath, string masterTemplateFilePath)
    {
      return this.GetEmailBody(templateFilePath, masterTemplateFilePath, new UTF8Encoding());
    }

    public string GetEmailBody(string templateFilePath, string masterTemplateFilePath, Encoding encoding)
    {
      string template = File.ReadAllText(templateFilePath, encoding);
      template = ProcessPlaceholders(template);

      if (String.IsNullOrEmpty(masterTemplateFilePath))
        return template;

      string mastertemplate = File.ReadAllText(masterTemplateFilePath, encoding);
      mastertemplate = mastertemplate.Replace("<%template%>", template);
      mastertemplate = ProcessPlaceholders(mastertemplate);
      return mastertemplate;
    }

    public void AddDataObject(string code, object dataObject)
    {
      if (this.placeholders == null)
        this.placeholders = new Dictionary<string, object>();

      this.placeholders.Add(code, dataObject);
    }

    #region Protected methods
    private string ProcessPlaceholders(string template)
    {
      Regex regex = new Regex("<%([^%])*%>");

      MatchCollection matches = regex.Matches(template);
      foreach (Match match in matches)
      {
        Placeholder placeholder = new Placeholder(match.Value);

        if (this.placeholders.ContainsKey(placeholder.RootObjectKey))
        {
          string value;
          if (placeholder.Fields.Length == 1)
          {
            value = this.placeholders[placeholder.RootObjectKey].ToString();
          }
          else
          {
            value = this.GetObjectPropertyValue(this.placeholders[placeholder.RootObjectKey], placeholder, 1);
          }

          template = template.Replace(placeholder.PlaceholderString, System.Net.WebUtility.HtmlEncode(value));
        }
        else
        {
          throw new InvalidDataException("Placeholder \"" + placeholder.PlaceholderString + "\" not found in data object collection.");
        }
      }

      return template;
    }

    private string GetObjectPropertyValue(object dataObject, Placeholder placeholder, int index)
    {
      if (index >= placeholder.Fields.Length)
      {
        throw new ArgumentOutOfRangeException("index");
      }

      string propertyName = placeholder.Fields[index];
      PropertyInfo p = dataObject.GetType().GetProperty(propertyName);
      if (p == null)
      {
        throw new ApplicationException("Field named \"" + propertyName + "\" not found.");
      }

      object value = p.GetValue(dataObject);

      if (index == placeholder.Fields.Length - 1)
      {
        if (!String.IsNullOrEmpty(placeholder.Format))
        {
          string[] tokens = placeholder.Format.Split("|".ToCharArray());
          if (value is bool)
          {
            if (tokens.Length != 2)
            {
              throw new ApplicationException("Field format \"" + placeholder.Format + "\" not valid.");
            }

            return (bool)value ? tokens[0] : tokens[1];
          }
          else if (value is string)
          {
            if (tokens.Length > 2)
            {
              throw new ApplicationException("Field format \"" + placeholder.Format + "\" not valid.");
            }
            else if (tokens.Length == 2)
            {
              return String.Format(String.IsNullOrEmpty((string)value) ? tokens[0] : tokens[1], value);
            }
            else
            {
              return String.Format(placeholder.Format, value);
            }
          }
          else if (value is DateTime)
          {
            return ((DateTime)value).ToString(placeholder.Format);
          }
          else if (value == null && tokens.Length == 2)
          {
            return tokens[1];
          }
          else
          {
            return String.Format(placeholder.Format, value);
          }
        } 
        else
        {
          return value != null ? value.ToString() : "";
        }
      } 
      else
      {
        return this.GetObjectPropertyValue(value, placeholder, index + 1);
      }
    }
    #endregion
  }
}

