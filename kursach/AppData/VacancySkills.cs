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
    
    public partial class VacancySkills
    {
        public int VacancyId { get; set; }
        public int SkillId { get; set; }
        public bool IsRequired { get; set; }
    
        public virtual Skills Skills { get; set; }
        public virtual Vacancies Vacancies { get; set; }
    }
}
