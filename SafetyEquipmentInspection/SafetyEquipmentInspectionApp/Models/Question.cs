﻿namespace SafetyEquipmentInspectionApp.Models
{
    public class Question
    {
        public virtual int EmployeeId { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Email { get; set; }
    }
}
