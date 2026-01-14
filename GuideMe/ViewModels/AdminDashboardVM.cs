using System.Collections.Generic;

namespace GuideMe.ViewModels
{
    public class AdminDashboardVM
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalGuides { get; set; }
        public int TotalVisitors { get; set; }
        
        // Data for charts
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlyBookingCounts { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();
    }
}
