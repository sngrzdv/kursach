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
    
    public partial class VacancyResponses
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VacancyResponses()
        {
            this.Interviews = new HashSet<Interviews>();
        }
    
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public int ResumeId { get; set; }
        public string Message { get; set; }
        public System.DateTime ResponseDate { get; set; }
        public int StatusId { get; set; }
        public string EmployerComment { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Interviews> Interviews { get; set; }
        public virtual ResponseStatuses ResponseStatuses { get; set; }
        public virtual Resumes Resumes { get; set; }
        public virtual Vacancies Vacancies { get; set; }
    }
}
