using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using BaseProject.DAO.Models;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace BaseProject.Test.Mocks
{
    public static class ServicesMock 
    {
        public static Mock<IDetectionService> CreateDetectionService() 
        {
            var serviceDetectionMock = new Mock<IDetectionService>();
            var deviceServiceMock = new Mock<IDeviceService>();
            deviceServiceMock.Setup(x => x.Type).Returns(Device.Desktop);
            serviceDetectionMock.Setup(x => x.Device).Returns(deviceServiceMock.Object);
            var platformServiceMock = new Mock<IPlatformService>();
            platformServiceMock.Setup(x => x.Name).Returns(Platform.Windows);
            platformServiceMock.Setup(x => x.Version).Returns(new Version("10.0.1"));
            platformServiceMock.Setup(x => x.Processor).Returns(Processor.x64);
            serviceDetectionMock.Setup(x => x.Platform).Returns(platformServiceMock.Object);
            var browserServiceMock = new Mock<IBrowserService>();
            browserServiceMock.Setup(x => x.Name).Returns(Browser.Chrome);
            browserServiceMock.Setup(x => x.Version).Returns(new Version("0.0.1"));
            serviceDetectionMock.Setup(x => x.Browser).Returns(browserServiceMock.Object);
            return serviceDetectionMock;
        }
    }
}
