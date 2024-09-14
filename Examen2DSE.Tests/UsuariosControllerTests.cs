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
    public class UsuariosControllerTests
    {
        private UsuariosController GetUsuariosController(AppDbContext context, IDistributedCache cache)
        {
            return new UsuariosController(context, cache);
        }

        [Fact]
        public async Task GetUsuarios_ReturnsUsuariosFromDatabase_WhenNoCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "UsuariosDbTest")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();

            context.Usuarios.Add(new Usuario { Id = 1, Nombre = "Kevin", Email = "kevin@example.com", Contraseña = "password123" });
            context.Usuarios.Add(new Usuario { Id = 2, Nombre = "Alex", Email = "alex@example.com", Contraseña = "password456" });
            await context.SaveChangesAsync();

            var controller = GetUsuariosController(context, cacheMock.Object);

            // Act
            var result = await controller.GetUsuarios();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var usuarios = Assert.IsAssignableFrom<IEnumerable<Usuario>>(okResult.Value);
            Assert.Equal(2, usuarios.Count());
        }

        [Fact]
        public async Task GetUsuario_ReturnsNotFound_WhenUsuarioDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "UsuariosDbTest_NotFound")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();
            var controller = GetUsuariosController(context, cacheMock.Object);

            // Act
            var result = await controller.GetUsuario(99); 

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostUsuario_CreatesNewUsuarioAndInvalidatesCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "UsuariosDbTest_Post")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();
            var controller = GetUsuariosController(context, cacheMock.Object);

            var newUsuario = new Usuario { Nombre = "Carlos", Email = "carlos@example.com", Contraseña = "password789" };

            // Act
            var result = await controller.PostUsuario(newUsuario);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdUsuario = Assert.IsType<Usuario>(createdAtActionResult.Value);
            Assert.Equal("Carlos", createdUsuario.Nombre);

            cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async Task DeleteUsuario_RemovesUsuarioAndInvalidatesCache()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "UsuariosDbTest_Delete")
                .Options;

            using var context = new AppDbContext(options);
            var cacheMock = new Mock<IDistributedCache>();

            var usuario = new Usuario { Nombre = "ToDelete", Email = "delete@example.com", Contraseña = "password000" };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var controller = GetUsuariosController(context, cacheMock.Object);

            // Act
            var result = await controller.DeleteUsuario(usuario.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(context.Usuarios.Find(usuario.Id)); 

            cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), default), Times.AtLeastOnce);
        }
    }
}
