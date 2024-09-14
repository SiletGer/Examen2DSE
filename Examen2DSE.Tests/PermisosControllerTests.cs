using Xunit;
using Moq;
using Examen2DSE.Controllers;
using Examen2DSE.Data;
using Examen2DSE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace Examen2DSE.Tests
{
    public class PermisosControllerTests
    {
        private PermisosController GetPermisosController(AppDbContext context, IDistributedCache cache)
        {
            return new PermisosController(context, cache);
        }

        [Fact]
        public async Task GetPermisos_ReturnsPermisosFromDatabase_WhenNoCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "PermisosDbTest")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();

            // Insertamos datos de prueba en la base de datos
            context.Permisos.Add(new Permiso { Id = 1, Nombre = "Read" });
            context.Permisos.Add(new Permiso { Id = 2, Nombre = "Write" });
            await context.SaveChangesAsync();

            var controller = GetPermisosController(context, cacheMock.Object);

            // Act
            var result = await controller.GetPermisos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var permisos = Assert.IsAssignableFrom<IEnumerable<Permiso>>(okResult.Value);
            Assert.Equal(2, permisos.Count());
        }

        [Fact]
        public async Task GetPermiso_ReturnsNotFound_WhenPermisoDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "PermisosDbTest_NotFound")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();
            var controller = GetPermisosController(context, cacheMock.Object);

            // Act
            var result = await controller.GetPermiso(99); 

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostPermiso_CreatesNewPermisoAndInvalidatesCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "PermisosDbTest_Post")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();
            var controller = GetPermisosController(context, cacheMock.Object);

            var newPermiso = new Permiso { Nombre = "Execute" };

            // Act
            var result = await controller.PostPermiso(newPermiso);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdPermiso = Assert.IsType<Permiso>(createdAtActionResult.Value);
            Assert.Equal("Execute", createdPermiso.Nombre);

            cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task DeletePermiso_RemovesPermisoAndInvalidatesCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "PermisosDbTest_Delete")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();

            var permiso = new Permiso { Nombre = "ToDelete" };
            context.Permisos.Add(permiso);
            await context.SaveChangesAsync();

            var controller = GetPermisosController(context, cacheMock.Object);

            // Act
            var result = await controller.DeletePermiso(permiso.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(context.Permisos.Find(permiso.Id)); 

            cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.AtLeastOnce);
        }
    }
}
