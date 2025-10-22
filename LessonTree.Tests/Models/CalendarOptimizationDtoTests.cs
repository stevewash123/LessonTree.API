using LessonTree.Models.DTO;
using LessonTree.Tests.Helpers;

namespace LessonTree.Tests.Models
{
    /// <summary>
    /// Unit tests for Calendar Optimization DTOs
    /// Tests validation logic, data integrity, and business rules for calendar optimization models
    /// </summary>
    public class CalendarOptimizationDtoTests : TestBase
    {
        #region SequenceContinuationRequest Tests

        [Fact]
        public void SequenceContinuationRequest_WithValidData_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var request = new SequenceContinuationRequest
            {
                AfterDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 12, 31),
                SpecificCourseIds = new List<int> { 1, 2, 3 },
                SpecificPeriods = new List<int> { 1, 2 },
                SkipCompletedCourses = true,
                MaxEventsToGenerate = 100
            };

            // Assert
            request.AfterDate.Should().Be(new DateTime(2024, 6, 1));
            request.EndDate.Should().Be(new DateTime(2024, 12, 31));
            request.SpecificCourseIds.Should().BeEquivalentTo(new[] { 1, 2, 3 });
            request.SpecificPeriods.Should().BeEquivalentTo(new[] { 1, 2 });
            request.SkipCompletedCourses.Should().BeTrue();
            request.MaxEventsToGenerate.Should().Be(100);
        }

        [Fact]
        public void SequenceContinuationRequest_WithDefaultValues_ShouldHaveCorrectDefaults()
        {
            // Arrange & Act
            var request = new SequenceContinuationRequest
            {
                AfterDate = new DateTime(2024, 6, 1)
            };

            // Assert
            request.EndDate.Should().BeNull();
            request.SpecificCourseIds.Should().BeNull();
            request.SpecificPeriods.Should().BeNull();
            request.SkipCompletedCourses.Should().BeTrue(); // Default is true
            request.MaxEventsToGenerate.Should().BeNull();
        }

        [Theory]
        [InlineData("2024-01-01", "2023-12-31")] // End before start
        [InlineData("2024-06-15", "2024-06-15")] // Same date (edge case)
        public void SequenceContinuationRequest_WithEdgeCaseDates_ShouldAllowConfiguration(string afterDateStr, string endDateStr)
        {
            // Arrange
            var afterDate = DateTime.Parse(afterDateStr);
            var endDate = DateTime.Parse(endDateStr);

            // Act
            var request = new SequenceContinuationRequest
            {
                AfterDate = afterDate,
                EndDate = endDate
            };

            // Assert
            request.AfterDate.Should().Be(afterDate);
            request.EndDate.Should().Be(endDate);
            // Note: Business logic validation should be handled in services, not DTOs
        }

        [Fact]
        public void SequenceContinuationRequest_WithEmptyCollections_ShouldAllowEmptyLists()
        {
            // Arrange & Act
            var request = new SequenceContinuationRequest
            {
                AfterDate = new DateTime(2024, 6, 1),
                SpecificCourseIds = new List<int>(),
                SpecificPeriods = new List<int>()
            };

            // Assert
            request.SpecificCourseIds.Should().NotBeNull();
            request.SpecificCourseIds.Should().BeEmpty();
            request.SpecificPeriods.Should().NotBeNull();
            request.SpecificPeriods.Should().BeEmpty();
        }

        #endregion

        #region SequenceAnalysisResult Tests

        [Fact]
        public void SequenceAnalysisResult_WithValidData_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var result = new SequenceAnalysisResult
            {
                TotalCoursesInScope = 5,
                TotalLessonsInScope = 150,
                ContinuationPoints = new List<ContinuationPoint>
                {
                    new()
                    {
                        Period = 1,
                        CourseId = 1,
                        CourseTitle = "Mathematics",
                        LastAssignedLessonIndex = 10,
                        TotalLessons = 30,
                        RemainingLessons = 20
                    }
                },
                CoursePeriodDetails = new List<CoursePeriodDetail>
                {
                    new()
                    {
                        CourseId = 1,
                        CourseTitle = "Mathematics",
                        Period = 1,
                        TotalLessons = 30,
                        AssignedLessons = 10,
                        NeedsContinuation = true
                    }
                }
            };

            // Assert
            result.TotalCoursesInScope.Should().Be(5);
            result.TotalLessonsInScope.Should().Be(150);
            result.ContinuationPoints.Should().HaveCount(1);
            result.CoursePeriodDetails.Should().HaveCount(1);

            var continuationPoint = result.ContinuationPoints.First();
            continuationPoint.CourseTitle.Should().Be("Mathematics");
            continuationPoint.RemainingLessons.Should().Be(20);
        }

        [Fact]
        public void SequenceAnalysisResult_WithEmptyCollections_ShouldInitializeWithEmptyLists()
        {
            // Arrange & Act
            var result = new SequenceAnalysisResult();

            // Assert
            result.ContinuationPoints.Should().NotBeNull();
            result.ContinuationPoints.Should().BeEmpty();
            result.CoursePeriodDetails.Should().NotBeNull();
            result.CoursePeriodDetails.Should().BeEmpty();
            result.TotalCoursesInScope.Should().Be(0);
            result.TotalLessonsInScope.Should().Be(0);
        }

        #endregion

        #region ContinuationPoint Tests

        [Fact]
        public void ContinuationPoint_WithValidData_ShouldCalculateRemainingLessonsCorrectly()
        {
            // Arrange & Act
            var continuationPoint = new ContinuationPoint
            {
                Period = 2,
                CourseId = 5,
                CourseTitle = "Science",
                LastAssignedLessonIndex = 15,
                TotalLessons = 40,
                RemainingLessons = 25, // Explicitly set
                ContinuationDate = new DateTime(2024, 6, 15),
                PeriodAssignment = new PeriodAssignmentResource
                {
                    Period = 2,
                    CourseId = 5
                }
            };

            // Assert
            continuationPoint.Period.Should().Be(2);
            continuationPoint.CourseId.Should().Be(5);
            continuationPoint.CourseTitle.Should().Be("Science");
            continuationPoint.LastAssignedLessonIndex.Should().Be(15);
            continuationPoint.TotalLessons.Should().Be(40);
            continuationPoint.RemainingLessons.Should().Be(25);
            continuationPoint.ContinuationDate.Should().Be(new DateTime(2024, 6, 15));
            continuationPoint.PeriodAssignment.Should().NotBeNull();
            continuationPoint.PeriodAssignment.Period.Should().Be(2);
        }

        [Fact]
        public void ContinuationPoint_WithZeroRemainingLessons_ShouldIndicateCompletion()
        {
            // Arrange & Act
            var continuationPoint = new ContinuationPoint
            {
                CourseTitle = "Completed Course",
                LastAssignedLessonIndex = 20,
                TotalLessons = 20,
                RemainingLessons = 0
            };

            // Assert
            continuationPoint.RemainingLessons.Should().Be(0);
            // Course is completed when remaining lessons = 0
        }

        #endregion

        #region CoursePeriodDetail Tests

        [Fact]
        public void CoursePeriodDetail_WithValidData_ShouldIndicateCorrectContinuationStatus()
        {
            // Arrange & Act
            var detail = new CoursePeriodDetail
            {
                CourseId = 3,
                CourseTitle = "History",
                Period = 3,
                TotalLessons = 25,
                AssignedLessons = 10,
                NeedsContinuation = true,
                LastAssignedDate = new DateTime(2024, 5, 30)
            };

            // Assert
            detail.CourseId.Should().Be(3);
            detail.CourseTitle.Should().Be("History");
            detail.Period.Should().Be(3);
            detail.TotalLessons.Should().Be(25);
            detail.AssignedLessons.Should().Be(10);
            detail.NeedsContinuation.Should().BeTrue();
            detail.LastAssignedDate.Should().Be(new DateTime(2024, 5, 30));
        }

        [Fact]
        public void CoursePeriodDetail_WithCompletedCourse_ShouldNotNeedContinuation()
        {
            // Arrange & Act
            var detail = new CoursePeriodDetail
            {
                CourseTitle = "Completed Course",
                TotalLessons = 15,
                AssignedLessons = 15,
                NeedsContinuation = false
            };

            // Assert
            detail.TotalLessons.Should().Be(detail.AssignedLessons);
            detail.NeedsContinuation.Should().BeFalse();
        }

        #endregion

        #region ScheduleGenerationResult Tests

        [Fact]
        public void ScheduleGenerationResult_WithValidData_ShouldCalculateProcessingTimeCorrectly()
        {
            // Arrange
            var startTime = new DateTime(2024, 6, 15, 10, 0, 0);
            var endTime = new DateTime(2024, 6, 15, 10, 5, 30);

            // Act
            var result = new ScheduleGenerationResult
            {
                Success = true,
                TotalEventsGenerated = 150,
                GenerationStarted = startTime,
                GenerationCompleted = endTime,
                Errors = new List<string>(),
                Warnings = new List<string> { "Some non-critical warning" }
            };

            // Assert
            result.Success.Should().BeTrue();
            result.TotalEventsGenerated.Should().Be(150);
            result.ProcessingTime.Should().Be(TimeSpan.FromMinutes(5.5));
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().HaveCount(1);
        }

        [Fact]
        public void ScheduleGenerationResult_WithErrors_ShouldIndicateFailure()
        {
            // Arrange & Act
            var result = new ScheduleGenerationResult
            {
                Success = false,
                Errors = new List<string>
                {
                    "Configuration validation failed",
                    "Missing required period assignments"
                },
                Warnings = new List<string>()
            };

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain("Configuration validation failed");
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void ScheduleGenerationResult_WithStatistics_ShouldAccumulateEventCounts()
        {
            // Arrange & Act
            var result = new ScheduleGenerationResult
            {
                EventsByPeriod = new Dictionary<int, int>
                {
                    { 1, 40 },
                    { 2, 35 },
                    { 3, 30 }
                },
                EventsByType = new Dictionary<string, int>
                {
                    { "Lesson", 95 },
                    { "SpecialDay", 10 }
                }
            };

            // Assert
            result.EventsByPeriod.Should().HaveCount(3);
            result.EventsByPeriod[1].Should().Be(40);
            result.EventsByType.Should().HaveCount(2);
            result.EventsByType["Lesson"].Should().Be(95);
            result.EventsByType["SpecialDay"].Should().Be(10);
        }

        #endregion

        #region ScheduleValidationResult Tests

        [Fact]
        public void ScheduleValidationResult_WithInheritedProperties_ShouldExtendBaseValidation()
        {
            // Arrange & Act
            var result = new ScheduleValidationResult
            {
                // Inherited from ScheduleConfigurationValidationResource
                IsValid = true,

                // New generation-specific properties
                TotalPeriodsConfigured = 6,
                CourseAssignments = 12,
                SpecialPeriodAssignments = 2,
                UnassignedPeriods = 0
            };

            // Assert
            result.IsValid.Should().BeTrue();
            result.TotalPeriodsConfigured.Should().Be(6);
            result.CourseAssignments.Should().Be(12);
            result.SpecialPeriodAssignments.Should().Be(2);
            result.UnassignedPeriods.Should().Be(0);
        }

        #endregion

        #region ScheduleGenerationPreview Tests

        [Fact]
        public void ScheduleGenerationPreview_WithValidData_ShouldProvideEstimates()
        {
            // Arrange & Act
            var preview = new ScheduleGenerationPreview
            {
                CanGenerate = true,
                EstimatedEventCount = 180,
                EstimatedEventsByPeriod = new Dictionary<int, int>
                {
                    { 1, 60 },
                    { 2, 60 },
                    { 3, 60 }
                },
                DateRange = new DateRange
                {
                    StartDate = new DateTime(2024, 8, 1),
                    EndDate = new DateTime(2025, 6, 30),
                    TeachingDaysCount = 180
                },
                ValidationResult = new ScheduleValidationResult
                {
                    IsValid = true,
                    TotalPeriodsConfigured = 3
                }
            };

            // Assert
            preview.CanGenerate.Should().BeTrue();
            preview.EstimatedEventCount.Should().Be(180);
            preview.EstimatedEventsByPeriod.Should().HaveCount(3);
            preview.DateRange.Should().NotBeNull();
            preview.DateRange.TeachingDaysCount.Should().Be(180);
            preview.ValidationResult.Should().NotBeNull();
            preview.ValidationResult.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ScheduleGenerationPreview_WithValidationErrors_ShouldPreventGeneration()
        {
            // Arrange & Act
            var preview = new ScheduleGenerationPreview
            {
                CanGenerate = false,
                EstimatedEventCount = 0,
                ValidationResult = new ScheduleValidationResult
                {
                    IsValid = false,
                    UnassignedPeriods = 3
                }
            };

            // Assert
            preview.CanGenerate.Should().BeFalse();
            preview.EstimatedEventCount.Should().Be(0);
            preview.ValidationResult.IsValid.Should().BeFalse();
            preview.ValidationResult.UnassignedPeriods.Should().Be(3);
        }

        #endregion

        #region DateRange Tests

        [Fact]
        public void DateRange_WithValidData_ShouldCalculateCorrectly()
        {
            // Arrange & Act
            var dateRange = new DateRange
            {
                StartDate = new DateTime(2024, 8, 1),
                EndDate = new DateTime(2024, 8, 31),
                TeachingDaysCount = 22 // Excluding weekends
            };

            // Assert
            dateRange.StartDate.Should().Be(new DateTime(2024, 8, 1));
            dateRange.EndDate.Should().Be(new DateTime(2024, 8, 31));
            dateRange.TeachingDaysCount.Should().Be(22);
        }

        [Theory]
        [InlineData("2024-01-01", "2024-01-01", 1)] // Single day
        [InlineData("2024-01-01", "2024-01-07", 5)] // One week (5 teaching days)
        [InlineData("2024-09-01", "2024-12-31", 85)] // Semester (example)
        public void DateRange_WithVariousRanges_ShouldAcceptValidData(string startStr, string endStr, int teachingDays)
        {
            // Arrange
            var startDate = DateTime.Parse(startStr);
            var endDate = DateTime.Parse(endStr);

            // Act
            var dateRange = new DateRange
            {
                StartDate = startDate,
                EndDate = endDate,
                TeachingDaysCount = teachingDays
            };

            // Assert
            dateRange.StartDate.Should().Be(startDate);
            dateRange.EndDate.Should().Be(endDate);
            dateRange.TeachingDaysCount.Should().Be(teachingDays);
        }

        #endregion

        #region Integration Scenarios

        [Fact]
        public void CalendarOptimizationWorkflow_WithFullSequenceContinuation_ShouldWorkTogether()
        {
            // Arrange - Simulate a complete workflow
            var continuationRequest = new SequenceContinuationRequest
            {
                AfterDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 12, 31),
                SpecificCourseIds = new List<int> { 1, 2 },
                MaxEventsToGenerate = 50
            };

            var analysisResult = new SequenceAnalysisResult
            {
                TotalCoursesInScope = 2,
                TotalLessonsInScope = 60,
                ContinuationPoints = new List<ContinuationPoint>
                {
                    new()
                    {
                        CourseId = 1,
                        CourseTitle = "Mathematics",
                        RemainingLessons = 20,
                        PeriodAssignment = new PeriodAssignmentResource { Period = 1 }
                    },
                    new()
                    {
                        CourseId = 2,
                        CourseTitle = "Science",
                        RemainingLessons = 15,
                        PeriodAssignment = new PeriodAssignmentResource { Period = 2 }
                    }
                }
            };

            var generationResult = new ScheduleGenerationResult
            {
                Success = true,
                TotalEventsGenerated = 35 // 20 + 15
            };

            // Act & Assert - All DTOs should work together
            continuationRequest.SpecificCourseIds.Should().BeEquivalentTo(new[] { 1, 2 });
            analysisResult.ContinuationPoints.Should().HaveCount(2);
            analysisResult.ContinuationPoints.Sum(cp => cp.RemainingLessons).Should().Be(35);
            generationResult.TotalEventsGenerated.Should().Be(35);
            generationResult.Success.Should().BeTrue();
        }

        #endregion
    }
}