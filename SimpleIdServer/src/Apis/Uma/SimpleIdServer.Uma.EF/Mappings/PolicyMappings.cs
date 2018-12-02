﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Uma.EF.Models;

namespace SimpleIdServer.Uma.EF.Mappings
{
    internal static class PolicyMappings
    {
        #region Public static methods

        public static void AddPolicyMappings(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Policy>()
                .ToTable("Policies")
                .HasKey(a => a.Id);
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.ResourceSetPolicies)
                .WithOne(p => p.Policy)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Claims)
                .WithOne(p => p.Policy)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Scopes)
                .WithOne(p => p.Policy)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
