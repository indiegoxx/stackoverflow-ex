// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class StackOverflowQuestion
    {
        public List<string> tags { get; set; }
        public bool is_answered { get; set; }
        public int view_count { get; set; }
        public int closed_date { get; set; }
        public int answer_count { get; set; }
        public int score { get; set; }
        public int last_activity_date { get; set; }
        public int creation_date { get; set; }
        public int last_edit_date { get; set; }
        public int question_id { get; set; }
        public string link { get; set; }
        public string closed_reason { get; set; }
        public string title { get; set; }
        public int? accepted_answer_id { get; set; }
        public string content_license { get; set; }
    }


    public class StackExchangeResponse
    {
        public List<StackOverflowQuestion> items { get; set; }
        public bool has_more { get; set; }
        public int quota_max { get; set; }
        public int quota_remaining { get; set; }
    }

    public class RecentQuestion
    {
        public string Title { get; set; }
        public DateTime Timestamp { get; set; }
    }

