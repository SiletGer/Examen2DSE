using Examen2DSE.Controllers;
using Examen2DSE.Data;
using Examen2DSE.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Examen2DSE.Tests
{
    public class RolesControllerTests
    {
        private RolesController GetRolesController(AppDbContext context, IDistributedCache cache)
        {
            return new RolesController(context, cache);
        }

        [Fact]
        public async Task GetRoles_ReturnsRolesFromDatabase_WhenNoCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "RolesDbTest")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();

            // Insertamos datos de prueba en la base de datos
            context.Roles.Add(new Rol { Id = 1, Nombre = "Admin" });
            context.Roles.Add(new Rol { Id = 2, Nombre = "User" });
            await context.SaveChangesAsync();

            var controller = GetRolesController(context, cacheMock.Object);

            // Act
            var result = await controller.GetRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var roles = Assert.IsAssignableFrom<IEnumerable<Rol>>(okResult.Value);
            Assert.Equal(2, roles.Count());
        }

        [Fact]
        public async Task GetRol_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "RolesDbTest_NotFound")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();
            var controller = GetRolesController(context, cacheMock.Object);

            // Act
            var result = await controller.GetRol(99); 

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostRol_CreatesNewRoleAndInvalidatesCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "RolesDbTest_Post")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();
            var controller = GetRolesController(context, cacheMock.Object);

            var newRol = new Rol { Nombre = "Manager" };

            // Act
            var result = await controller.PostRol(newRol);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdRole = Assert.IsType<Rol>(createdAtActionResult.Value);
            Assert.Equal("Manager", createdRole.Nombre);

            cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task DeleteRol_RemovesRoleAndInvalidatesCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "RolesDbTest_Delete")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();

            var rol = new Rol { Nombre = "ToDelete" };
            context.Roles.Add(rol);
            await context.SaveChangesAsync();

            var controller = GetRolesController(context, cacheMock.Object);

            // Act
            var result = await controller.DeleteRol(rol.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(context.Roles.Find(rol.Id)); 

            cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.AtLeastOnce);
        }
    }
}
