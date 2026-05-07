namespace ClientPlatform.Common
{
    public class CustomSearchFilter
    {
        /// <summary>
        /// Field name (e.g. "UserName", "Alliance.Name")
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Operator: eq, neq, contains, startswith, endswith, gt, lt, gte, lte
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Value to search
        /// </summary>
        public string Value { get; set; }
    }
}
