// Decompiled with JetBrains decompiler
// Type: CBT.CBTServer.Utils.TestObject
// Assembly: SZCBTServer, Version=2.10.0.614, Culture=neutral, PublicKeyToken=null
// MVID: 2B1BE590-A367-451A-9783-32AE3678D8FB
// Assembly location: D:\SZDEC\SZCBTServer.exe

namespace ZslCustomsAssist.Utils
{
  internal class TestObject
  {
    private string testString;
    private int testInt;
    private bool testBoolean;
    private char testChar;

    public TestObject()
    {
      this.testBoolean = true;
      this.testChar = 'c';
      this.testInt = 1234567890;
      this.testString = nameof (testString);
    }

    public string toJsonString() => "{testBoolean:" + this.testBoolean.ToString() + ";testChar:" + this.testChar.ToString() + ";testInt" + (object) this.testInt + "; testString:" + this.testString + ";}";
  }
}
