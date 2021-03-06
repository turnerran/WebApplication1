using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using WebApi.Helpers;
using Xunit;
using System.Linq;
using WebApplication1.Models.Domains;
using WebApi.Services;

namespace Tests
{
    public class SchedueledTaskServiceTests : BaseTestService
    {
        private DataContext _databaseContext;
        public SchedueledTaskServiceTests() : base()
        {
            SetDatabaseContext();
        }


        [Fact]
        public async void IsGettingTaskWithValidIdReturnsTrue()
        {
            var loggerMock = new Mock<ILogger<SchedueledTaskService>>();

            var service = new SchedueledTaskService(loggerMock.Object, _databaseContext);
            var res = await service.GetTaskById(1);
            Assert.True(res.Url == "www.1.co.il");

        }

        [Fact]
        public async void IsGettingTaskWithValidIdReturnsFalse()
        {
            var loggerMock = new Mock<ILogger<SchedueledTaskService>>();

            var service = new SchedueledTaskService(loggerMock.Object, _databaseContext);
            var res = await service.GetTaskById(0);
            Assert.Null(res);
        }

        [Fact]
        public async void IsMarkingTaskCompletedReturnsTrue()
        {
            var loggerMock = new Mock<ILogger<SchedueledTaskService>>();

            var service = new SchedueledTaskService(loggerMock.Object, _databaseContext);
            var task = await service.MarkTaskAsCompleted(1);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async void IsGettingOnlyTasksOverDueReturnsTrue()
        {
            var loggerMock = new Mock<ILogger<SchedueledTaskService>>();

            var service = new SchedueledTaskService(loggerMock.Object, _databaseContext);
            var tasks = await service.GetTaskOverDue();

            Assert.True(tasks.Count() == 3);
        }

        [Fact]
        public async void IsCreatingValidNewTaskSucceeds()
        {
            var loggerMock = new Mock<ILogger<SchedueledTaskService>>();
            var dateTimeMock = new Mock<IDateTimeService>();
            var service = new SchedueledTaskService(loggerMock.Object, _databaseContext);
            var task = new SchedueledTask
            {
                FireEventTime = dateTimeMock.Object.ConvertInputToSchedueledTime(0, 1, 0),
                IsCompleted = false,
                Url = $"www.blabla.co.il",
            };

            var savedTask = await service.Create(task);

            Assert.True(savedTask.Id == 11);
            Assert.True(!savedTask.IsCompleted);
        }

        [Fact]
        public async void IsCreatingNullTaskThrowsError()
        {
            var loggerMock = new Mock<ILogger<SchedueledTaskService>>();

            var service = new SchedueledTaskService(loggerMock.Object, _databaseContext);

            await Assert.ThrowsAsync<ArgumentNullException>(async() => await service.Create(null));
        }

        [Fact]
        public async void IsCreatingTaskWithExistingIdThrowsError()
        {
            var loggerMock = new Mock<ILogger<SchedueledTaskService>>();
            var dateTimeMock = new Mock<IDateTimeService>();
            var service = new SchedueledTaskService(loggerMock.Object, _databaseContext);

            var task = new SchedueledTask
            {
                FireEventTime = dateTimeMock.Object.ConvertInputToSchedueledTime(0, 0, 0),
                Id = 1,
                IsCompleted = false,
                Url = "www.blabla.co.il",
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async() => await service.Create(task));
        }

        private async Task SetDatabaseContext()
        {
            var dateTimeMock = new Mock<IDateTimeService>();
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new DataContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.SchedueledTasks.CountAsync() <= 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    databaseContext.SchedueledTasks.Add(new SchedueledTask
                    {
                        FireEventTime = dateTimeMock.Object.ConvertInputToSchedueledTime(0, 0, 0),
                        Id = i,
                        IsCompleted = i <= 7,
                        Url = $"www.{i}.co.il",
                    });
                    await databaseContext.SaveChangesAsync();
                }
            }

            _databaseContext = databaseContext;
        }
    }
}