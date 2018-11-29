#region copyright
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
    internal static class PolicyRuleMappings
    {
        #region Public static methods

        public static void AddPolicyRuleMappings(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicyRule>()
                .ToTable("PolicyRules")
                .HasKey(p => p.Id);
            modelBuilder.Entity<PolicyRule>()
                .HasMany(p => p.Claims)
                .WithOne(p => p.PolicyRule)
                .HasForeignKey(p => p.PolicyRuleId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PolicyRule>()
                .HasMany(p => p.Scopes)
                .WithOne(p => p.PolicyRule)
                .HasForeignKey(p => p.PolicyRuleId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PolicyRule>()
                .HasMany(p => p.ClientIdsAllowed)
                .WithOne(p => p.PolicyRule)
                .HasForeignKey(p => p.PolicyRuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
