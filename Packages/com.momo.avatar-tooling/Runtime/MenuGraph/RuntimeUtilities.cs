using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    public static class RuntimeUtilities
    {
        // Edits of VRC's VRCExpressionParameters code
        public static int CalculateTotalCostOfParamters(this List<MenuGraphParamter> avatarParamters)
        {
            int num = 0;
            foreach (MenuGraphParamter paramter in avatarParamters)
            {
                if (paramter.networkSynced) num += paramter.valueType.TypeCost();
            }
            return num;
        }
        public static int TypeCost(this VRCExpressionParameters.ValueType type)
        {
            if (type != VRCExpressionParameters.ValueType.Bool)
            {
                return 8;
            }
            return 1;
        }
        public static bool IsWithinBudget(this List<MenuGraphParamter> avatarParamters)
        {
            string failureReason;
            return IsWithinBudget(avatarParamters, out failureReason);
        }
        public static bool IsWithinBudget(this List<MenuGraphParamter> avatarParamters, out string failureReason)
        {
            if (avatarParamters.CalculateTotalCostOfParamters() > VRCExpressionParameters.MAX_PARAMETER_COST)
            {
                failureReason = "Synced parameters use too much memory (remove parameters or use bools which use less memory)";
                return false;
            }

            if (avatarParamters != null && avatarParamters.Count > VRCExpressionParameters.MAX_PARAMETER_COUNT)
            {
                failureReason = $"Too many parameters (there is a {VRCExpressionParameters.MAX_PARAMETER_COUNT} parameter max)";
                return false;
            }

            failureReason = null;
            return true;
        }
    }
}
