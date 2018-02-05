
using System.Reflection;
using System;
using NRules.Fluent;
using NRules;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Dynamic;
using System.ComponentModel;
using NRule_1_24.Domain;
using Newtonsoft.Json.Linq;
using NRule_1_24.Rule;
namespace NRule_1_24

{
    class Program
    {
        static string jsonRuleFilePath = @".\JsonFile\message-rules.json";
        static string jsonDataFilePath = @".\JsonFile\telemetry.json";
        static string jsonRuleFilePath2 = @".\JsonFile\msgrule2.json";
        private static void Main(string[] args)
        {

            loadAlarmRuleCatalogRule();

            Console.ReadLine();
        }


        private static void loadAlarmRuleCatalogRule()
        {
            // Load JSON
            Dictionary<string, List<AlarmRules>> ruleItem = getRuleItemListFromFile(jsonRuleFilePath);
            // Put ruleItem to  CustomRuleRepository
            var repository = new CustomRuleRepository(ruleItem);
            repository.LoadRules();

            Console.WriteLine("Loaded rules:");
            foreach (var rule in repository.GetRules())
            {
                Console.WriteLine(rule.Name);
            }
            Console.WriteLine();


            //Compile rules
            ISessionFactory factory = repository.Compile();
            
            //Create a working session
            ISession session = factory.CreateSession();
            
            // ExpandoObjeact lsit
            List<ExpandoObject> explist = getTelemetryJsonFiletoExpando(jsonDataFilePath);

            //Insert Data to RuleEngine
            insertDatatoRuleEngine(explist, session);

            //Run RuleEngine
            session.Fire();

        }
        private static dynamic readJsonFile(string jsonFilepath)
        {
            dynamic jsonObj = null;
            using (StreamReader sr = new StreamReader(jsonFilepath))
            {
                string jsonstr = sr.ReadToEnd();
                jsonObj = JsonConvert.DeserializeObject(jsonstr);
            }
            return jsonObj;
        }

        private static Dictionary<string, List<AlarmRules>> getRuleItemListFromFile(string jsonFilepath)
        {
            dynamic jsonObj = readJsonFile(jsonFilepath);
            var msgIdandAlarmObj = new Dictionary<string, List<AlarmRules>>();         
            try
            {
                foreach (var rules in jsonObj)
                {
                    List<MessageIds> msgIdslsit = new List<MessageIds>();
                    List<AlarmRules> alarmRuleslist = new List<AlarmRules>();

                    MessageIds msgId = new MessageIds();
                    msgId.MessageId = rules.MessageId;
                    msgId.MessageName = rules.MessageName;
                    msgIdslsit.Add(msgId);

                    if (rules.AlarmRules != null)
                    {
                        foreach (var alarmRule in rules.AlarmRules)
                        {
                            List<RuleItem> ruleItemslist = new List<RuleItem>();
                            AlarmRules alrs = new AlarmRules();
                            alrs.AlarmRuleId = alarmRule.AlarmRuleId;
                            alrs.AlarmRuleName = alarmRule.AlarmRuleName;
                            alarmRuleslist.Add(alrs);
                            if (alarmRule.AlarmRuleItems != null)
                            {
                                foreach (var item in alarmRule.AlarmRuleItems)
                                {
                                    RuleItem ri = new RuleItem();
                                    ri.ElmentName = item.ElmentName;
                                    ri.DataType = item.DataType;
                                    ri.Operation = item.Operators;
                                    // Do Filter Data Type;
                                    filterDataType(ri, item);
                                    ri.Logic = item.Logic;                               
                                    ruleItemslist.Add(ri);
                                    alrs.RuleItemlist = ruleItemslist;
                                }
                                msgId.AlarmRuleslist = alarmRuleslist;
                            }
                        }
                    }
                    else 
                    {
                        Console.WriteLine(msgId.MessageId + " " + "AlarmRules is null");
                    }
                    msgIdandAlarmObj.Add(msgId.MessageId, msgId.AlarmRuleslist);
                   
                }

            }
            catch (JsonException e)
            {
                Console.WriteLine(e.Message);
            }
            return msgIdandAlarmObj;
        }
        private static List<ExpandoObject> getTelemetryJsonFiletoExpando(string jsonFilepath)
        {
            List<ExpandoObject> explist = new List<ExpandoObject>();
            dynamic jsonObj = readJsonFile(jsonFilepath);
            try
            {
                foreach (var json in jsonObj)
                {
                    ExpandoObject exp = new ExpandoObject();
       
                    addProperty(exp, "MessageName", json.MessageName);
                    addProperty(exp, "MessageId", json.MessageId);
                    foreach (JProperty msgpayload in json.MessagePayload)
                    {
                        addProperty(exp, msgpayload.Name, msgpayload.Value);
                    }
                    explist.Add(exp);
                }

            }
            catch (JsonException e)
            {
                Console.WriteLine(e.Message);
            }
            return explist;

        }
        private static RuleItem filterDataType(RuleItem ri, dynamic items)
        {
            int value = items.DataType;
            switch (value)
            {
                case 0:
                    ri.RightboolValue = items.StringRightValue;
                    break;
                case 1:
                    ri.RightNumbericValue = items.StringRightValue;
                    break;
                case 2:
                    ri.RightStringValue = items.StringRightValue;
                    break;
                default:
                    throw new ArgumentNullException();
            }
            return ri;
        }
        private static void insertDatatoRuleEngine(List<ExpandoObject> explist, ISession session)
        {
            foreach (dynamic data in explist)
            {
                session.Insert(data);
            }
        }
        public static void addProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary 
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}