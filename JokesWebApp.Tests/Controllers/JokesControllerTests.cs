using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JokesWebApp.Controllers;
using JokesWebApp.Data;
using JokesWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace JokesWebApp.Tests.Controllers
{
    [TestFixture]
    public class JokesControllerTests
    {
        private static ApplicationDbContext NewContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var ctx = new ApplicationDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        private static async Task SeedAsync(ApplicationDbContext ctx)
        {
            ctx.Joke.AddRange(
                new Joke { Id = 1, JokeQuestion = "Why did the chicken cross the road?", JokeAnswer = "To get to the other side." },
                new Joke { Id = 2, JokeQuestion = "Best dev joke?", JokeAnswer = "It works on my machine." },
                new Joke { Id = 3, JokeQuestion = "What contains a 'SearchTerm' inside?", JokeAnswer = "This one does!" }
            );
            await ctx.SaveChangesAsync();
        }

        [Test]
        public async Task Index_ReturnsViewWithAllJokes()
        {
            using var ctx = NewContext(nameof(Index_ReturnsViewWithAllJokes));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.Index();

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;

            Assert.That(view.Model, Is.InstanceOf<List<Joke>>());
            var model = (List<Joke>)view.Model;

            Assert.That(model.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task ShowSearchForm_ReturnsView()
        {
            using var ctx = NewContext(nameof(ShowSearchForm_ReturnsView));
            var controller = new JokesController(ctx);

            var result = await controller.ShowSearchForm();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task ShowSearchResults_FiltersByQuestionSubstring()
        {
            using var ctx = NewContext(nameof(ShowSearchResults_FiltersByQuestionSubstring));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.ShowSearchResults("SearchTerm");

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;

            Assert.That(view.ViewName, Is.EqualTo("Index"));

            Assert.That(view.Model, Is.InstanceOf<List<Joke>>());
            var model = (List<Joke>)view.Model;

            Assert.That(model.Count, Is.EqualTo(1));
            Assert.That(model.Single().Id, Is.EqualTo(3));
        }

        [Test]
        public async Task Details_NullId_ReturnsNotFound()
        {
            using var ctx = NewContext(nameof(Details_NullId_ReturnsNotFound));
            var controller = new JokesController(ctx);

            var result = await controller.Details(null);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Details_UnknownId_ReturnsNotFound()
        {
            using var ctx = NewContext(nameof(Details_UnknownId_ReturnsNotFound));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.Details(999);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Details_KnownId_ReturnsViewWithModel()
        {
            using var ctx = NewContext(nameof(Details_KnownId_ReturnsViewWithModel));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.Details(2);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;

            Assert.That(view.Model, Is.InstanceOf<Joke>());
            var model = (Joke)view.Model;

            Assert.That(model.Id, Is.EqualTo(2));
        }

        [Test]
        public void Create_Get_ReturnsView()
        {
            using var ctx = NewContext(nameof(Create_Get_ReturnsView));
            var controller = new JokesController(ctx);

            var result = controller.Create();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Create_Post_ValidModel_SavesAndRedirects()
        {
            using var ctx = NewContext(nameof(Create_Post_ValidModel_SavesAndRedirects));
            var controller = new JokesController(ctx);

            var newJoke = new Joke { Id = 100, JokeQuestion = "Q?", JokeAnswer = "A!" };

            var result = await controller.Create(newJoke);

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;

            Assert.That(redirect.ActionName, Is.EqualTo(nameof(JokesController.Index)));

            var saved = await ctx.Joke.FindAsync(100);
            Assert.That(saved, Is.Not.Null);
        }

        [Test]
        public async Task Create_Post_InvalidModel_ReturnsSameViewWithModel()
        {
            using var ctx = NewContext(nameof(Create_Post_InvalidModel_ReturnsSameViewWithModel));
            var controller = new JokesController(ctx);
            controller.ModelState.AddModelError("JokeQuestion", "Required");

            var newJoke = new Joke { Id = 101, JokeQuestion = null, JokeAnswer = "A!" };

            var result = await controller.Create(newJoke);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;

            Assert.That(view.Model, Is.SameAs(newJoke));

            var notSaved = await ctx.Joke.FindAsync(101);
            Assert.That(notSaved, Is.Null);
        }

        [Test]
        public async Task Edit_Get_NullId_ReturnsNotFound()
        {
            using var ctx = NewContext(nameof(Edit_Get_NullId_ReturnsNotFound));
            var controller = new JokesController(ctx);

            var result = await controller.Edit(null);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_UnknownId_ReturnsNotFound()
        {
            using var ctx = NewContext(nameof(Edit_Get_UnknownId_ReturnsNotFound));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.Edit(999);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_KnownId_ReturnsViewWithModel()
        {
            using var ctx = NewContext(nameof(Edit_Get_KnownId_ReturnsViewWithModel));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.Edit(1);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;

            Assert.That(view.Model, Is.InstanceOf<Joke>());
            var model = (Joke)view.Model;

            Assert.That(model.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            using var ctx = NewContext(nameof(Edit_Post_IdMismatch_ReturnsNotFound));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var joke = new Joke { Id = 1, JokeQuestion = "Q", JokeAnswer = "A" };
            var result = await controller.Edit(999, joke);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_InvalidModel_ReturnsSameViewWithModel()
        {
            using var ctx = NewContext(nameof(Edit_Post_InvalidModel_ReturnsSameViewWithModel));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);
            controller.ModelState.AddModelError("JokeQuestion", "Required");

            var joke = new Joke { Id = 1, JokeQuestion = null, JokeAnswer = "A" };
            var result = await controller.Edit(1, joke);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;

            Assert.That(view.Model, Is.SameAs(joke));
        }

        [Test]
        public async Task Edit_Post_Valid_UpdatesAndRedirects()
        {
            using var ctx = NewContext(nameof(Edit_Post_Valid_UpdatesAndRedirects));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var joke = await ctx.Joke.FindAsync(2);
            joke!.JokeAnswer = "Updated";

            var result = await controller.Edit(2, joke);

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;

            Assert.That(redirect.ActionName, Is.EqualTo(nameof(JokesController.Index)));

            var updated = await ctx.Joke.FindAsync(2);
            Assert.That(updated!.JokeAnswer, Is.EqualTo("Updated"));
        }

        [Test]
        public async Task Delete_Get_NullId_ReturnsNotFound()
        {
            using var ctx = NewContext(nameof(Delete_Get_NullId_ReturnsNotFound));
            var controller = new JokesController(ctx);

            var result = await controller.Delete(null);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_UnknownId_ReturnsNotFound()
        {
            using var ctx = NewContext(nameof(Delete_Get_UnknownId_ReturnsNotFound));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.Delete(999);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_KnownId_ReturnsViewWithModel()
        {
            using var ctx = NewContext(nameof(Delete_Get_KnownId_ReturnsViewWithModel));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.Delete(1);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;

            Assert.That(view.Model, Is.InstanceOf<Joke>());
            var model = (Joke)view.Model;

            Assert.That(model.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteConfirmed_RemovesEntityAndRedirects()
        {
            using var ctx = NewContext(nameof(DeleteConfirmed_RemovesEntityAndRedirects));
            await SeedAsync(ctx);
            var controller = new JokesController(ctx);

            var result = await controller.DeleteConfirmed(1);

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;

            Assert.That(redirect.ActionName, Is.EqualTo(nameof(JokesController.Index)));

            var deleted = await ctx.Joke.FindAsync(1);
            Assert.That(deleted, Is.Null);
        }
    }
}
