﻿namespace Contracts.ServiceModules.Emails.Analytics.Models;

public record LoadVsServiceLimitReport(List<int> SegmentedCounts, List<int> CumulativeCounts);
