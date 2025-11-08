using GuideMe.Repositories;
using GuideMe.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Identity.Controllers
{
    [Area(SD.IdentityArea)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<Visitor> _visitorRepository;
        private readonly IRepository<Guide> _guideRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<UserOTP> _userOTPRepository;

        public AccountController(

            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IRepository<Visitor> visitorRepository,
            IRepository<Guide> guideRepository,
            SignInManager<ApplicationUser> signInManager,
            IRepository<UserOTP> userOTPRepository


            )
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _visitorRepository = visitorRepository;
            _guideRepository = guideRepository;
            _signInManager = signInManager;
            _userOTPRepository = userOTPRepository;
        }


        [HttpGet]
        public IActionResult VisitorRegister()
        {

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area="Main"});
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> VisitorRegister(VisitorRegisterVm visitorRegisterVm)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                return View(visitorRegisterVm);
            }

            ApplicationUser user = new ApplicationUser()
            {

                FirstName = visitorRegisterVm.FirstName,
                LastName = visitorRegisterVm.LastName,
                UserName = visitorRegisterVm.UserName,
                Email = visitorRegisterVm.Email,
                Country = visitorRegisterVm.Country,
                PhoneNumber = visitorRegisterVm.PhoneNumber,
                Role = UserRole.Visitor


            };

            var result = await _userManager.CreateAsync(user, visitorRegisterVm.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
                return View(visitorRegisterVm);
            }

            await _userManager.AddToRoleAsync(user, SD.VisitorRole);

            // add user to be visitor

            Visitor visitor = new Visitor()
            {
                Passport = visitorRegisterVm.Passport,
                ApplicationUserId = user.Id,
                visitorStatus = VisitorStatus.Available
            };

            //add visitor
            await _visitorRepository.CreateAsync(visitor);
            await _visitorRepository.CommitAsync();


            //confirm message 

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);
            string htmlBody = $@"<div style='font-family:Arial,Helvetica,sans-serif;background-color:#f4f4f4;padding:40px 0;'>
                                <div style='max-width:480px;margin:auto;background:#ffffff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);padding:25px;text-align:center;'>
                                    <h2 style='color:#0078d7;margin-bottom:10px;'>Email Confirmation</h2>
                                    <p style='font-size:15px;color:#333;margin-bottom:25px;'>Welcome to GuideMe 👋<br>Please confirm your email address by clicking the button below:</p>
        
                                    <a href='{link}' target='_blank' style='display:inline-block;padding:12px 24px;background-color:#0078d7;color:#ffffff;text-decoration:none;font-size:16px;font-weight:bold;border-radius:6px;'>Confirm Email</a>
        
                                    <p style='font-size:13px;color:#555;margin-top:25px;'>If you didn’t create an account, you can safely ignore this email.</p>
        
                                    <hr style='margin:25px 0;border:none;border-top:1px solid #e0e0e0;'>
                                    <p style='font-size:13px;color:#0078d7;margin:0;font-weight:bold;'>GuideMe</p>
                                </div>
                            </div>";
            await _emailSender.SendEmailAsync(user.Email, "Confim your Email", htmlBody);
            TempData["success-notification"] = "User created Successfully,check your inbox to confirm";

            return RedirectToAction("index", "Home", new { area = "Main" });
        }


        [HttpGet]

        public IActionResult GuideRegister()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Main" });
            }
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> GuideRegister(GuideRegisterVm guideRegisterVm)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                return View(guideRegisterVm);
            }

            ApplicationUser user = new ApplicationUser()
            {

                FirstName = guideRegisterVm.FirstName,
                LastName = guideRegisterVm.LastName,
                UserName = guideRegisterVm.UserName,
                Email = guideRegisterVm.Email,
                Country = guideRegisterVm.Country,
                PhoneNumber = guideRegisterVm.PhoneNumber,
                Role = UserRole.Guide


            };

            var result = await _userManager.CreateAsync(user, guideRegisterVm.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
                return View(guideRegisterVm);
            }

            await _userManager.AddToRoleAsync(user, SD.VisitorRole);

            // add user to be guide

            Guide guide = new Guide()
            {
                NationalId = guideRegisterVm.NationalId,
                LicenseNumber = guideRegisterVm.LicenseNumber,
                YearsOfExperience = guideRegisterVm.YearsOfExperience,
                ApplicationUserId = user.Id

            };



            //add related Guide

            await _guideRepository.CreateAsync(guide);
            await _guideRepository.CommitAsync();


            //confirm message 

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);
            string htmlBody = $@"<div style='font-family:Arial,Helvetica,sans-serif;background-color:#f4f4f4;padding:40px 0;'>
                                <div style='max-width:480px;margin:auto;background:#ffffff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);padding:25px;text-align:center;'>
                                    <h2 style='color:#0078d7;margin-bottom:10px;'>Email Confirmation</h2>
                                    <p style='font-size:15px;color:#333;margin-bottom:25px;'>Welcome to GuideMe 👋<br>Please confirm your email address by clicking the button below:</p>
        
                                    <a href='{link}' target='_blank' style='display:inline-block;padding:12px 24px;background-color:#0078d7;color:#ffffff;text-decoration:none;font-size:16px;font-weight:bold;border-radius:6px;'>Confirm Email</a>
        
                                    <p style='font-size:13px;color:#555;margin-top:25px;'>If you didn’t create an account, you can safely ignore this email.</p>
        
                                    <hr style='margin:25px 0;border:none;border-top:1px solid #e0e0e0;'>
                                    <p style='font-size:13px;color:#0078d7;margin:0;font-weight:bold;'>GuideMe</p>
                                </div>
                            </div>";

            await _emailSender.SendEmailAsync(user.Email, "Confim your Email", htmlBody);
            TempData["success-notification"] = "User created Successfully,check your inbox to confirm";

            return RedirectToAction("index", "Home", new { area = "Main" });
        }

        [HttpGet]

        public async Task<IActionResult> ConfirmEmail(string token, string userId)
        {

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }


            var result = await _userManager.ConfirmEmailAsync(user, token);


            if (!result.Succeeded)
            {
                TempData["error-notification"] = "Can't Confirm Your Eamil";
            }
            else
            {
                TempData["success-notification"] = "Email Confirmed Successfully";
            }


            return RedirectToAction("Index", "Home", new { area = "Main" });
        }


        [HttpGet]

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Main" });
            }

            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Login(LoginVM loginVM)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                return View(loginVM);
            }

            var user = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail) ??
                     await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);

            if (user is null)
            {
                TempData["error-notification"] = "Invalid Email or Password";
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);

            if (!result.Succeeded)
            {
                TempData["error-notification"] = "Invalid Email or Password";
                return View(loginVM);
            }

            if (user.EmailConfirmed == false)
            {
                TempData["error-notification"] = "Email Not Confirmed";
                return View(loginVM);
            }

            TempData["success-notification"] = "Login Successfully";

            return RedirectToAction("Index", "Home", new { area = "Main" });


        }



        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Main" });
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVm resendEmailConfirmationVm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));
                return View(resendEmailConfirmationVm);
            }

            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationVm.UserNameOrEmail) ??
                      await _userManager.FindByNameAsync(resendEmailConfirmationVm.UserNameOrEmail);

            if (user is null)
            {
                TempData["error-notification"] = "Invalied Email Or UserName";
                return View(resendEmailConfirmationVm);
            }

            if (user.EmailConfirmed == true)
            {
                TempData["error-notification"] = "Email Or UserName Already confirmed";
                return View(resendEmailConfirmationVm);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);
            string htmlBody = $@"<div style='font-family:Arial,Helvetica,sans-serif;background-color:#f4f4f4;padding:40px 0;'>
                            <div style='max-width:480px;margin:auto;background:#ffffff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);padding:25px;text-align:center;'>
                                <h2 style='color:#0078d7;margin-bottom:10px;'>Email Confirmation</h2>
                                <p style='font-size:15px;color:#333;margin-bottom:25px;'>Welcome to GuideMe 👋<br>Please confirm your email address by clicking the button below:</p>
        
                                <a href='{link}' target='_blank' style='display:inline-block;padding:12px 24px;background-color:#0078d7;color:#ffffff;text-decoration:none;font-size:16px;font-weight:bold;border-radius:6px;'>Confirm Email</a>
        
                                <p style='font-size:13px;color:#555;margin-top:25px;'>If you didn’t create an account, you can safely ignore this email.</p>
        
                                <hr style='margin:25px 0;border:none;border-top:1px solid #e0e0e0;'>
                                <p style='font-size:13px;color:#0078d7;margin:0;font-weight:bold;'>GuideMe</p>
                            </div>
                        </div>";


            await _emailSender.SendEmailAsync(user.Email, "Reconfirm Email", htmlBody);

            TempData["success-notification"] = "Check inbox to confirm";

            return RedirectToAction("Login", "Account", new { area = "identity" });
        }


        [HttpGet]
        public IActionResult ForgetPassword()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Main" });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));
                return View(forgetPasswordVM);
            }

            var user = await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOrEmail) ??
                      await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOrEmail);


            if (user is null)
            {
                TempData["error-notification"] = "invalid userName Or Password";
                return View(forgetPasswordVM);
            }

            // create an otp
            var otpNumber = new Random().Next(1000, 9999);

            UserOTP OTP = new UserOTP()
            {
                ApplicationUserId = user.Id,
                OTPNumber = otpNumber,
                ValidTo = DateTime.UtcNow.AddDays(1)
            };

            await _userOTPRepository.CreateAsync(OTP);
            await _userOTPRepository.CommitAsync();

            //send email to the user mailbox to confirm it in the resetAction

            string htmlBody = $@"<div style='font-family:Arial,Helvetica,sans-serif;background-color:#f4f4f4;padding:40px 0;'>
                                <div style='max-width:480px;margin:auto;background:#ffffff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);padding:25px;text-align:center;'>
                                    <h2 style='color:#0078d7;margin-bottom:10px;'>Password Reset Verification</h2>
                                    <p style='font-size:15px;color:#333;margin-bottom:20px;'>Use the OTP code below to reset your password:</p>
                                    <div style='display:inline-block;padding:10px 22px;font-size:24px;font-weight:bold;letter-spacing:4px;background:#f0f8ff;border:1px solid #0078d7;border-radius:6px;color:#0078d7;margin-bottom:20px;'>{otpNumber}</div>
                                    <p style='font-size:13px;color:#555;'>This code will expire in 1 day. Do not share it with anyone.</p>
                                    <hr style='margin:25px 0;border:none;border-top:1px solid #e0e0e0;'>
                                     <hr style='margin:25px 0;border:none;border-top:1px solid #e0e0e0;'>
                                       <p style='font-size:13px;color:#0078d7;margin:0;font-weight:bold;'>GuideMe</p>
                                </div>
                            </div>";

            await _emailSender.SendEmailAsync(user.Email, "Reset Password OTP", htmlBody);
            TempData["success-notification"] = "Check your inbox for OTP number";

            return RedirectToAction("ResetPassword", "Account", new { area = "Identity", userId = user.Id });


        }


        [HttpGet]

        public IActionResult ResetPassword(string userId)
        {

            return View(new ResetPasswordVM { ApplicationUserId = userId });
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid)
            {

                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));
                return View(resetPasswordVM);
            }

            var user = await _userManager.FindByIdAsync(resetPasswordVM.ApplicationUserId);

            if (user is null)
            {
                TempData["error-notification"] = "Can't reset Password ,Invalid Username Or Email";

                //i redirect the  user to  the  forgetpassword because he may modify the  url

                return RedirectToAction("ForgetPassword", "Account", new { area = "Identity" });
            }

            var userOTP = (await _userOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id)).OrderBy(e => e.Id).LastOrDefault();



            if (resetPasswordVM.OTPNumber != userOTP?.OTPNumber)

            {
                TempData["error-notification"] = "Wrong OTP Number";
                return View(resetPasswordVM);
            }

            if (userOTP.ValidTo < DateTime.UtcNow)
            {
                TempData["error-notification"] = "Expired OTP !";
                return View(resetPasswordVM);
            }

            TempData["success-notification"] = " valid OTP number";

            return RedirectToAction("ChangePassword", "Account", new { area = "Identity", userId = user.Id });

        }

        [HttpGet]

        public IActionResult ChangePassword(string userId)
        {

            return View(new ChangePasswordVM { ApplicationUserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                return View(changePasswordVM);
            }

            var user = await _userManager.FindByIdAsync(changePasswordVM.ApplicationUserId);

            if (user is null)
            {
                TempData["error-notification"] = "invalid user name!!";
                return RedirectToAction("ChangePassword", "Accout", new { area = "Identity" });

            }
            var temptoken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _userManager.ResetPasswordAsync(user, temptoken, changePasswordVM.Password);

            TempData["success-notification"] = "password Changed SuccuessFully";


            return RedirectToAction("Login", "Account", new { area = "Identity" });

        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

    }
}
