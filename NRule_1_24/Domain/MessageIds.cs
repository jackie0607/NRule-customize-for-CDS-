using System;
using System.Collections.Generic;
using System.Text;

namespace NRule_1_24.Domain
{
    public class MessageIds
    {
        public string MessageName;
        public string MessageId;
        public List<AlarmRules> AlarmRuleslist;
    }
    public class AlarmRules
    {
        public string AlarmRuleName;
        public string AlarmRuleId;
        public List<RuleItem> RuleItemlist;
    }
    public class RuleItem
    {       
        public string AlarmRules;
        public string ElmentName;
        public int DataType;
        public string RightStringValue = null;
        public bool RightboolValue;
        public double RightNumbericValue;
        public string Operation;
        public string Logic;
        public bool result;
    }
}
