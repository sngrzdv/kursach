//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace kursach.AppData
{
    using System;
    using System.Collections.Generic;
    
    public partial class Interviews
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public System.DateTime InterviewDate { get; set; }
        public string Location { get; set; }
        public string OnlineMeetingLink { get; set; }
        public string Notes { get; set; }
        public bool IsCompleted { get; set; }
        public string Result { get; set; }
    
        public virtual VacancyResponses VacancyResponses { get; set; }
    }
}
