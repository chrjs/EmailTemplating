using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmailTemplating;
using System.IO;

namespace UnitTestProject1
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

    public bool ValBool
    {
      get { return true; }
    }
  }

  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod1()
    {
      EmailTemplate et = new EmailTemplate();
      et.AddDataObject("MyData", new DataClass());
      File.WriteAllText("email-body.html", et.GetEmailBody("template1.html", "master.html"));
    }
  }
}
