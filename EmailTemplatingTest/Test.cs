using NUnit.Framework;
using System;
using EmailTemplating;
using System.Reflection;
using System.IO;

namespace EmailTemplatingTest
{
  public class DataClass
  {
    public string ValString
    {
      get { return "Pippo"; }
    }

    public int ValInt
    {
      get { return 100; }
    }

    public DateTime ValDateTime
    {
      get { return DateTime.UtcNow; }
    }
  }

  [TestFixture()]
  public class Test
  {
    [Test()]
    public void TestCase()
    {
      EmailTemplate et = new EmailTemplate();
      et.AddDataObject("MyData", new DataClass());
      File.WriteAllText("email-body.html", et.GetEmailBody("template1.html", "master.html"));
    }
  }
}

