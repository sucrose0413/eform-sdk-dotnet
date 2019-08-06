/*
The MIT License (MIT)

Copyright (c) 2007 - 2019 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Microting.eForm.Infrastructure.Data.Entities
{
    public partial class units : BaseEntity
    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Id { get; set; }

        public int? MicrotingUid { get; set; }

        public int? OtpCode { get; set; }

        public int? CustomerNo { get; set; }
//
//        public int? version { get; set; }
//
//        [StringLength(255)]
//        public string workflow_state { get; set; }
//
//        public DateTime? created_at { get; set; }
//
//        public DateTime? updated_at { get; set; }

        [ForeignKey("site")]
        public int? SiteId { get; set; }

        public virtual sites Site { get; set; }
        
        public void Create(MicrotingDbAnySql dbContext)
        {
            WorkflowState = Constants.Constants.WorkflowStates.Created;
            Version = 1;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;

            dbContext.units.Add(this);
            dbContext.SaveChanges();

            dbContext.unit_versions.Add(MapUnitVersions(this));
            dbContext.SaveChanges();
        }

        public void Update(MicrotingDbAnySql dbContext)
        {
            units unit = dbContext.units.FirstOrDefault(x => x.Id == Id);

            if (unit == null)
            {
                throw new NullReferenceException($"Could not find Unit with Id: {Id}");
            }

            unit.SiteId = SiteId;
            unit.MicrotingUid = MicrotingUid;
            unit.OtpCode = OtpCode;
            unit.CustomerNo = CustomerNo;

            if (dbContext.ChangeTracker.HasChanges())
            {
                unit.Version += 1;
                unit.UpdatedAt = DateTime.Now;

                dbContext.unit_versions.Add(MapUnitVersions(unit));
                dbContext.SaveChanges();
            }
        }

        public void Delete(MicrotingDbAnySql dbContext)
        {
            
            units unit = dbContext.units.FirstOrDefault(x => x.Id == Id);

            if (unit == null)
            {
                throw new NullReferenceException($"Could not find Unit with Id: {Id}");
            }

            unit.WorkflowState = Constants.Constants.WorkflowStates.Removed;

            if (dbContext.ChangeTracker.HasChanges())
            {
                unit.Version += 1;
                unit.UpdatedAt = DateTime.Now;

                dbContext.unit_versions.Add(MapUnitVersions(unit));
                dbContext.SaveChanges();
            }
        }

        
        
        private unit_versions MapUnitVersions(units units)
        {
            unit_versions unitVer = new unit_versions();
            unitVer.WorkflowState = units.WorkflowState;
            unitVer.Version = units.Version;
            unitVer.CreatedAt = units.CreatedAt;
            unitVer.UpdatedAt = units.UpdatedAt;
            unitVer.MicrotingUid = units.MicrotingUid;
            unitVer.SiteId = units.SiteId;
            unitVer.CustomerNo = units.CustomerNo;
            unitVer.OtpCode = units.OtpCode;

            unitVer.UnitId = units.Id; //<<--

            return unitVer;
        }
    }
}