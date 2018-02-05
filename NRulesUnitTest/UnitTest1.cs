using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using NRule_1_24.Domain;
using NRule_1_24.Rule;
using System.Dynamic;
using System;

namespace NRulesUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestComplieBoolRule()
        {
            Assert.IsFalse(testBoolRule(false, "AND", true));
            Assert.IsTrue(testBoolRule(true, "AND", true));

            Assert.IsTrue(testBoolRule(false, "OR", true));

            Assert.IsTrue(testBoolRule(false, "XOR", true));

            Assert.IsFalse(testBoolRule(true, "XOR", true));
        }
        [TestMethod]
        public void TestmsgMatch()
        {
            /*List 123 124 125*/
            List<string> msgIdList = new List<string>();
            msgIdList.Add("123");
            msgIdList.Add("124");
            msgIdList.Add("125");
            /*126*/
            ExpandoObject o = new ExpandoObject();
            addProperty(o, "MessageId", "126");

            /*123*/
            ExpandoObject o1 = new ExpandoObject();
            addProperty(o1, "MessageId", "123");

            Assert.IsFalse(msgIdMatch(o, msgIdList));
            Assert.IsTrue(msgIdMatch(o1, msgIdList));
        }
        [TestMethod]
        public void TestvalueMatch()
        {
            /* Expando Object o, A=10*/
            ExpandoObject o = new ExpandoObject();
            addProperty(o, "A", "10");
            /*Expando Object o1, B=-20*/
            ExpandoObject o1 = new ExpandoObject();
            addProperty(o1, "B", "-20");
            Assert.IsTrue(valueMatch(o, "A", 30, "<"));
            Assert.IsTrue(valueMatch(o, "A", 30, "<="));
            Assert.IsTrue(valueMatch(o, "A", 10, "="));
            Assert.IsTrue(valueMatch(o, "A", 10, ">="));
            Assert.IsTrue(valueMatch(o, "A", 9, ">"));
            Assert.IsTrue(valueMatch(o, "A", 9.9, ">"));
            Assert.IsTrue(valueMatch(o, "A", 9.9, "!="));
            Assert.IsTrue(valueMatch(o, "A", 9, "!="));

            Assert.IsTrue(valueMatch(o1, "B", 9.9, "!="));
            Assert.IsTrue(valueMatch(o1, "B", 9, "<"));
            Assert.IsTrue(valueMatch(o1, "B", -20, "="));

            Assert.IsFalse(valueMatch(o1, "B", 10, ">"));
            Assert.IsFalse(valueMatch(o1, "B", 10, ">="));
            Assert.IsFalse(valueMatch(o1, "A", 10, ">="));
            Assert.IsFalse(valueMatch(o1, "A", 10, ">="));
        }
        [TestMethod]
        public void TeststrMatch()
        {
            /*Expando Object o, Owner ABC.Inc*/
            ExpandoObject o = new ExpandoObject();
            addProperty(o, "Owner", "ABC.Inc");
            /*Expando Object o, Owner ABC.Inc*/
            ExpandoObject o1 = new ExpandoObject();
            addProperty(o1, "Owner", "Jackie.Inc");
            Assert.IsTrue(stringMatch(o, "Owner", "ABC.Inc", "="));
            Assert.IsTrue(stringMatch(o1, "Owner", "Jackie.Inc", "="));
            Assert.IsTrue(stringMatch(o, "Owner", "Jackie.Inc", "!="));
            Assert.IsTrue(stringMatch(o1, "Owner", "ABC.Inc", "!="));


            Assert.IsFalse(stringMatch(o, "Owner", "Jackie.Inc", "="));
            Assert.IsFalse(stringMatch(o1, "Owner", "ABC.Inc", "="));
        }
        [TestMethod]
        public void TestboolMatch()
        {
            /*Expando Object o, Owner ABC.Inc*/
            ExpandoObject o = new ExpandoObject();
            addProperty(o, "CO2_TOO_HIGH_ALERT", true);

            Assert.IsTrue(boolMatch(o, "CO2_TOO_HIGH_ALERT", true, "="));
            Assert.IsTrue(boolMatch(o, "CO2_TOO_HIGH_ALERT", false, "!="));

            Assert.IsFalse(boolMatch(o, "CO2_TOO_HIGH_ALERT", false, "="));
            Assert.IsFalse(boolMatch(o, "CO2_TOO_HIGH_ALERT", true, "!="));
        }
        private Dictionary<string, List<AlarmRules>> getRuleItemListFromFile(string jsonFilepath)
        {
            Dictionary<string, List<AlarmRules>> ruleItemDictionary = new Dictionary<string, List<AlarmRules>>();
            return ruleItemDictionary;
        }
        private bool msgIdMatch(ExpandoObject o, List<string> msgIdList)
        {
            string msgstr = "";
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey("MessageId"))
            {
                msgstr = expandoDict["MessageId"].ToString();

            }
            return msgIdList.Contains(msgstr);
        }
        private bool valueMatch(ExpandoObject o, string name, double inputvalue, string op)
        {
            bool result = false;
            double value = 0;
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey(name))
            {

                if (double.TryParse(expandoDict[name].ToString(), out value))
                {
                    switch (op)
                    {
                        case "=":
                            result = (value == inputvalue);
                            break;
                        case ">":
                            result = (value > inputvalue);
                            break;
                        case "<":
                            result = (value < inputvalue);
                            break;
                        case ">=":
                            result = (value >= inputvalue);
                            break;
                        case "<=":
                            result = (value <= inputvalue);
                            break;
                        case "!=":
                            result = (value != inputvalue);
                            break;
                        default:
                            throw new ArgumentNullException();
                    }
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            return result;
        }
        private bool stringMatch(ExpandoObject o, string name, string inputvalue, string op)        
        {
            bool result = false;
            string str = "";
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey(name))
            {
                str = expandoDict[name].ToString();
            }
            switch (op)
            {
                case "=":
                    result = str.Equals(inputvalue);
                    break;
                case "!=":
                    result = str != inputvalue;
                    break;
                default:
                    throw new ArgumentNullException();
            }
            return result;
        }
        private bool boolMatch(ExpandoObject o, string name, bool inputvalue, string op)
        {
            bool result = false;
            bool trueOrfalse = false;
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey(name))
            {
                bool.TryParse(expandoDict[name].ToString(), out trueOrfalse);
            }
            switch (op)
            {
                case "=":
                    result = trueOrfalse.Equals((inputvalue));
                    break;
                case "!=":
                    result = trueOrfalse.Equals(!(inputvalue));
                    break;
                default:
                    throw new ArgumentNullException();
            }
            return result;
        }
        private bool testBoolRule(bool left, string op, bool right)
        {
            return CustomRuleRepository.ComplieBoolRule(left, op, right);
        }
        public static void addProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}
