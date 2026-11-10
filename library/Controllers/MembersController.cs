using Npgsql;
using library.Data;
using library.Models;
using Microsoft.AspNetCore.Mvc;

namespace library.Controllers;

public class MembersController : Controller
{
    private readonly MemberRepository _members;
    private readonly LoanRepository _loans;

    // ASP.NET Core hands in the repositories, because Program.cs registered them.
    public MembersController(MemberRepository members, LoanRepository loans)
    {
        _members = members;
        _loans = loans;
    }

    // GET /Members : the list of members.
    public async Task<IActionResult> Index()
    {
        var members = await _members.GetAllAsync();
        return View(members);                           // Views/Members/Index.cshtml
    }

    // GET /Members/Create : an empty form.
    public IActionResult Create()
    {
        return View(new Member());
    }

    // POST /Members/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (!ModelState.IsValid)                        // 1. a rule on the form was broken
        {
            return View(member);                        // 2. show the form again, with what was typed
        }

        try
        {
            await _members.AddAsync(member);            // 3. save the new member
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)   // 4. another member already uses this email
        {
            ModelState.AddModelError("Email", "Another member already uses this email.");        // 5. a message under the Email box
            return View(member);                                                                 // 6. show the form again, with what was typed
        }

        return RedirectToAction(nameof(Index));         // 7. the member was saved: go back to the list
    }

    // GET /Members/Edit/5 : the form, filled with member 5.
    public async Task<IActionResult> Edit(long id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return View(member);
    }

    // POST /Members/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Member member)
    {
        if (!ModelState.IsValid)                        // 1. a rule on the form was broken
        {
            return View(member);                        // 2. show the form again, with what was typed
        }

        try
        {
            await _members.UpdateAsync(member);         // 3. save the changes
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)   // 4. another member already uses this email
        {
            ModelState.AddModelError("Email", "Another member already uses this email.");        // 5. a message under the Email box
            return View(member);                                                                 // 6. show the form again, with what was typed
        }

        return RedirectToAction(nameof(Index));         // 7. the changes were saved: go back to the list
    }

    // GET /Members/Delete/5 : ask "are you sure?"
    public async Task<IActionResult> Delete(long id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return View(member);
    }

    // POST /Members/Delete/5 : the delete was confirmed.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            await _members.DeleteAsync(id);             // 1. delete the member
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)   // 2. the database refused because of a foreign key
        {
            var member = await _members.GetByIdAsync(id);     // 3. load the member again, to show them on the page
            if (member == null)                               // 4. there is no such member
            {
                return NotFound();                            // 5. answer "not found"
            }
            ViewBag.Error = "This member has loans on record, so the database will not delete them.";   // 6. the message the page will show
            return View("Delete", member);                    // 7. show the Delete page again, with the member
        }

        return RedirectToAction(nameof(Index));         // 8. the member was deleted: go back to the list
    }

    // GET /Members/Details/5 : one member and their loans.
    public async Task<IActionResult> Details(long id)
    {
        var member = await _members.GetByIdAsync(id);   // 1. load the member
        if (member == null)                             // 2. there is no such member
        {
            return NotFound();                          // 3. answer "not found"
        }

        // 4. put the member and their loans together for the page
        var details = new MemberDetails
        {
            Member = member,
            Loans = await _loans.GetByMemberAsync(id)
        };

        return View(details);                           // 5. show the page
    }
}
