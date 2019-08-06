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
    public partial class site_workers : BaseEntity
    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Id { get; set; }

        [ForeignKey("site")]
        public int? SiteId { get; set; }

        [ForeignKey("worker")]
        public int? WorkerId { get; set; }

        public int? MicrotingUid { get; set; }

//        public int? version { get; set; }
//
//        [StringLength(255)]
//        public string workflow_state { get; set; }
//
//        public DateTime? created_at { get; set; }
//
//        public DateTime? updated_at { get; set; }

        public virtual sites Site { get; set; }

        public virtual workers Worker { get; set; }

        public void Create(MicrotingDbAnySql dbContext)
        {
            WorkflowState = Constants.Constants.WorkflowStates.Created;
            Version = 1;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;

            dbContext.site_workers.Add(this);
            dbContext.SaveChanges();

            dbContext.site_worker_versions.Add(MapSiteWorkerVersions(this));
            dbContext.SaveChanges();
        }

        public void Update(MicrotingDbAnySql dbContext)
        {
            site_workers siteWorkers = dbContext.site_workers.FirstOrDefault(x => x.Id == Id);

            if (siteWorkers == null)
            {
                throw new NullReferenceException($"Could not find site worker tish Id: {Id}");
            }

            siteWorkers.SiteId = SiteId;
            siteWorkers.WorkerId = WorkerId;
            siteWorkers.MicrotingUid = MicrotingUid;

            if (dbContext.ChangeTracker.HasChanges())
            {
                siteWorkers.Version += 1;
                siteWorkers.UpdatedAt = DateTime.Now;

                dbContext.site_worker_versions.Add(MapSiteWorkerVersions(siteWorkers));
                dbContext.SaveChanges();
            }
        }

        public void Delete(MicrotingDbAnySql dbContext)
        {
            site_workers siteWorkers = dbContext.site_workers.FirstOrDefault(x => x.Id == Id);

            if (siteWorkers == null)
            {
                throw new NullReferenceException($"Could not find site worker tish Id: {Id}");
            }

            siteWorkers.WorkflowState = Constants.Constants.WorkflowStates.Removed;
            
            if (dbContext.ChangeTracker.HasChanges())
            {
                siteWorkers.Version += 1;
                siteWorkers.UpdatedAt = DateTime.Now;

                dbContext.site_worker_versions.Add(MapSiteWorkerVersions(siteWorkers));
                dbContext.SaveChanges();
            }
        }
        
        
        private site_worker_versions MapSiteWorkerVersions(site_workers site_workers)
        {
            site_worker_versions siteWorkerVer = new site_worker_versions();
            siteWorkerVer.WorkflowState = site_workers.WorkflowState;
            siteWorkerVer.Version = site_workers.Version;
            siteWorkerVer.CreatedAt = site_workers.CreatedAt;
            siteWorkerVer.UpdatedAt = site_workers.UpdatedAt;
            siteWorkerVer.MicrotingUid = site_workers.MicrotingUid;
            siteWorkerVer.SiteId = site_workers.SiteId;
            siteWorkerVer.WorkerId = site_workers.WorkerId;

            siteWorkerVer.SiteWorkerId = site_workers.Id; //<<--

            return siteWorkerVer;
        }

    }
}