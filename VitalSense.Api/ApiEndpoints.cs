namespace VitalSense.Api.Endpoints;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class Users
    {
        private const string Base = $"{ApiBase}/auth";

        public const string Login = $"{Base}/login";
        public const string Register = $"{Base}/register";
        public const string RefreshToken = $"{Base}/refresh";
        public const string Me = $"{Base}/me";
    }

    public static class Clients
    {
        private const string Base = $"{ApiBase}/clients";
        public const string Create = $"{Base}";
        public const string GetAll = $"{Base}";
        public const string GetById = $"{Base}/{{clientId}}";
        public const string Edit = $"{Base}/{{clientId}}";
        public const string Delete = $"{Base}/{{clientId}}";
        public const string Search = $"{Base}/search";
    }

    public static class MealPlans
    {
    private const string Base = $"{ApiBase}/meal-plans";
    public const string Create = $"{Base}";
    public const string GetById = $"{Base}/{{mealPlanId}}";
    public const string GetByClientId = $"{Base}/client/{{clientId}}";
    public const string GetActiveByClientId = $"{Base}/{{clientId}}/active";
    }
    
    public static class Tasks
    {
        private const string Base = $"{ApiBase}/tasks";
        public const string Create = $"{Base}";
        public const string GetAll = $"{Base}";
        public const string GetById = $"{Base}/{{taskId}}";
        public const string ToggleComplete = $"{Base}/{{taskId}}/toggle";
        public const string Delete = $"{Base}/{{taskId}}";
    }

    public static class Appointments
    {
        private const string Base = $"{ApiBase}/appointments";
        public const string Create = $"{Base}";
        public const string GetAll = $"{Base}";
        public const string GetById = $"{Base}/{{appointmentId}}";
        public const string Edit = $"{Base}/{{appointmentId}}";
        public const string Delete = $"{Base}/{{appointmentId}}";
        public const string GetByDate = $"{Base}/date/{{date}}";
        public const string GetByRange = $"{Base}/range/{{from}}/{{to}}";
    }

    public static class Health
    {
        private const string Base = $"{ApiBase}/health";
        public const string Get = $"{Base}";
    }

    public static class Dashboard
    {
        private const string Base = $"{ApiBase}/dashboard";
        public const string Metrics = $"{Base}/metrics";
    }

    public static class Integrations
    {
        private const string Base = $"{ApiBase}/integrations";
        
        public static class Google
        {
            private const string GoogleBase = $"{Base}/google";
            public const string Authorize = $"{GoogleBase}/authorize";
            public const string Callback = $"{GoogleBase}/callback";
            public const string Status = $"{GoogleBase}/status";
            public const string Disconnect = $"{GoogleBase}/disconnect";
        }
        
        public static class GoogleCalendar
        {
            private const string CalendarBase = $"{Base}/google-calendar";
            public const string SyncAppointment = $"{CalendarBase}/sync-appointment/{{appointmentId:guid}}";
            public const string UnsyncAppointment = $"{CalendarBase}/unsync-appointment/{{appointmentId:guid}}";
            public const string ValidateAppointment = $"{CalendarBase}/validate-appointment/{{appointmentId:guid}}";
        }
    }
}