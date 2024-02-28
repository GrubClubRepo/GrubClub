<!DOCTYPE html>
<!--[if lt IE 7 ]><html class="ie ie6" lang="en"><![endif]-->
<!--[if IE 7 ]><html class="ie ie7" lang="en"><![endif]-->
<!--[if IE 8 ]><html class="ie ie8" lang="en"><![endif]-->

 <html lang="en">
    <!--<![endif]-->

<head>
   <!-- Basic Page Needs
    ================================================== -->
    <% Response.StatusCode = 404 %>
<meta charset="utf-8">
<title>error page</title>
<meta name="description" content="@ViewBag.Description">

<!-- Mobile Specific
 ================================================== -->
<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1">


<!-- CSS
 ================================================== -->

<!-- Prefetch DNS for external assets (Twitter widgets etc). -->
<link rel="dns-prefetch" href="//ajax.googleapis.com">
<link rel="dns-prefetch" href="//fonts.googleapis.com">
<link rel="dns-prefetch" href="//themes.googleusercontent.com">
<link rel="dns-prefetch" href="//www.google-analytics.com">


<link rel="stylesheet" href="/Content/css/screen.css">
<link href='http://fonts.googleapis.com/css?family=Source+Sans+Pro:400,300,600,700,900' rel='stylesheet' type='text/css'>



<script src="../Scripts/jquery-1.7.2.min.js" type="text/javascript"></script>
<script src="../bower_components/modernizr/modernizr.js"></script>
<script src="../bower_components/jquery/dist/jquery.min.js"></script>
<script src="../Scripts/jquery-ui-1.10.4.min.js" type="text/javascript"></script>

<link rel="stylesheet" type="text/css" href="../Content/css/jquery-ui-1.10.3.min.css")" />

<script src="http://maps.google.com/maps/api/js?sensor=false"></script>
<script src="../bower_components/chosen_v1.3.0/chosen.jquery.min.js"></script>
<script src="../bower_components/svg4everybody/svg4everybody.min.js"></script>
<script src="../bower_components/owl-carousel2/dist/owl.carousel.min.js"></script>
<script src="../bower_components/nouislider/distribute/jquery.nouislider.all.min.js"></script>

<!-- legacy.js is datepicker support for ie8 -->
<script src="../bower_components/pickadate/lib/compressed/legacy.js"></script>*
<script src="../bower_components/pickadate/lib/compressed/picker.js"></script>
<script src="../bower_components/pickadate/lib/compressed/picker.date.js"></script>

<script src="../bower_components/nanoscroller/bin/javascripts/jquery.nanoscroller.min.js"></script>
<script src="../bower_components/magnific-popup/dist/jquery.magnific-popup.min.js"></script>
<script src="../bower_components/jquery.countdown/dist/jquery.countdown.js"></script>
<script src="../Scripts/init.min.js")"></script>

<script src="../Content/js/twitter.js" type="text/javascript"></script>
<!-- FB Advertising Pixel
   ==================================================== -->



    <style>
        #navigation {
            float: left;
            margin: 10px -1px 0 0;
        }

            #navigation ul li {
                float: left;
                position: relative;
            }


                #navigation ul li a {
                    color: #a9a9a8;
                    font-weight: 700;
                    font-size: 17px;
                    font-size: 1.0625rem;
                    line-height: 1.41176;
                    -moz-transition: all 0.2s ease;
                    -o-transition: all 0.2s ease;
                    -webkit-transition: all 0.2s ease;
                    transition: all 0.2s ease;
                }

                    #navigation ul li a:hover,
                    #navigation .toplevel {
                        height: 25px;
                    }

            #navigation ul ul {
                opacity: 0;
                padding: 6px 8px 8px 8px;
                margin: 0px 0 0 21px;
                filter: alpha(opacity=0);
                position: absolute;
                top: -99999px;
                left: 0;
                background: #F5F6F0;
                z-index: 999;
                border-radius: 5px;
            }

                #navigation ul ul li a {
                    padding: 3px 6px 1px 6px;
                    margin: 1px 0;
                    display: block;
                    font-family: Arial, sans-serif;
                    font-weight: normal;
                    line-height: 16px;
                    width: 119px;
                    text-align: left;
                    background: none;
                }

                #navigation ul ul ul {
                    position: absolute;
                    top: -99999px;
                    left: 100%;
                    opacity: 0;
                    margin: -10px 0 0 0px;
                    z-index: 999;
                    border-radius: 3px;
                }

                    #navigation ul ul ul li a {
                    }

            #navigation ul li:hover > ul {
                opacity: 1;
                position: absolute;
                top: 100%;
                left: -5px;
            }

            #navigation ul ul li:hover > ul {
                position: absolute;
                top: 0;
                left: 100%;
                opacity: 1;
                z-index: 497;
                background: #fff border: 0;
            }

            #navigation ul ul li:hover > a {
                border-radius: 2px;
                background: white !important;
                font-weight: normal !important;
            }
    </style>
    
</head>
 <body style="margin-top:-25px;">
    <!--<![endif]-->

<!-- Header -->
<header class="header-main">
    <div class="container">
        <div class="justifize justifize--middle">
            <div class="justifize__box">
                <div class="tableize tableize--full tableize--middle">
                    <div class="tableize__cell">
                        <div>
                            <a class="site-logo " href="/Home/Index"">
                                
                                <svg version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px"
                                     width="134px" height="36px" viewBox="0 0 134 36" enable-background="new 0 0 134 36" xml:space="preserve">
                                <path d="M79.872,28.815c2.885,0,6.993-1.365,9.461-4.733c0.061,3.04,1.797,4.357,4.016,4.357
        c1.509,0,4.053-0.479,6.114-3.366c0.384,2.34,1.973,3.366,3.936,3.366c1.581,0,3.688-0.489,5.57-3.573
        c0.264,2.369,1.581,3.573,3.728,3.573c1.523,0,3.903-0.492,5.882-3.454c0.49,2.177,2.211,3.454,5.109,3.454
        c7.979,0,10.313-8.313,10.313-12.715c0-4.777-1.807-6.432-4.441-6.432c-0.752,0-1.544,0.113-3.087,1.053l1.807-8.464l-5.57,0.752
        l-4.027,18.847h-0.001c-0.563,1.918-1.58,3.31-3.049,3.31c-1.053,0-1.542-0.339-1.542-1.392c0-0.376,0.037-0.789,0.15-1.204
        l2.709-12.789h-5.419l-2.561,12.075c-0.564,1.918-1.543,3.31-3.012,3.31c-0.864,0-1.128-0.677-1.128-1.542
        c0-0.338,0.038-0.678,0.112-1.054l2.71-12.789h-5.419L99.672,21.48h-0.001c-0.564,1.918-1.919,3.31-3.388,3.31
        c-1.053,0-1.543-0.339-1.543-1.392c0-0.376,0.038-0.789,0.15-1.204L99.22,1.881l-5.57,0.752L89.621,21.48
        c-0.009,0.043-0.012,0.078-0.021,0.121c-1.764,2.269-4.302,3.603-7.206,3.603c-3.651,0-4.517-2.407-4.517-5.868
        c0-7.674,3.651-17.567,7.829-17.567c1.693,0,2.296,1.091,2.296,2.294c0,1.467-0.828,3.009-1.732,3.46
        c0.717,0.865,1.544,0.978,2.146,0.978c1.921,0,2.636-1.467,2.636-3.46C91.052,1.43,88.267,0,84.766,0
        c-9.861, 0-13.137,12.752-13.137,19.561C71.629,26.332,74.303,28.815,79.872,28.815z M123.913,22.194l2.07-9.554
        c0.413-0.527,1.693-0.677,2.22-0.677c1.355,0,1.996,1.768,1.996,3.611c0,3.95-1.958,9.216-4.404,9.216
        c-1.317,0-1.995-0.563-1.995-1.73C123.8,22.796,123.838,22.495,123.913,22.194z M25.031,28.212l3.011-14.294
        c0.941-0.865,1.581-1.43,2.749-1.43c1.091,0,0.075,2.483,2.183,2.483c1.807,0,2.71-1.429,2.71-2.746
        c0-1.467-0.903-2.821-2.598-2.821c-1.807,0-3.237,1.166-4.554,2.332l0.489-2.332h-5.42l-3.99,18.808H25.031z M14.98,12.452
        l-1.619,7.147c-0.79,1.053-1.807,1.391-3.199,1.391c-3.124,0-4.366-2.896-4.366-6.206c0-6.019,4.216-13.015,8.356-13.015
        c2.409,0,2.597,2.069,2.597,2.67c0,0.94-0.753,2.784-1.657,3.084c0.716,0.79,1.995,0.978,2.598,0.978c1.92,0,2.635-1.467,2.635-3.46
        c0-3.611-3.5-5.041-7.001-5.041C4.931,0,0,7.298,0,14.483c0,5.229,1.919,9.63,8.092,9.63c0.301,0,2.409-0.15,4.592-1.354
        l-1.505,6.733c-0.903,3.913-2.033,4.627-3.764,4.627c-1.619,0-2.372-1.241-2.372-2.558c0-1.393,0.866-2.858,2.635-2.858
        c0.753,0,1.317,0.188,2.258,0.978c0.151-0.564,0.226-1.053,0.226-1.505c0-2.407-2.183-2.521-3.688-2.521
        c-2.974,0-4.592,2.596-4.592,5.154c0,2.558,1.656,5.19,5.232,5.19c6.173,0,8.431-3.611,9.636-9.366l3.086-14.145L14.98,12.452z
         M34.892,23.925c0,3.159,1.77,4.514,4.028,4.514c1.581,0,3.689-0.489,5.571-3.573c0.263,2.369,1.581,3.573,3.726,3.573
        c1.524,0,3.904-0.492,5.883-3.455c0.49,2.178,2.21,3.455,5.108,3.455c7.98,0,10.314-8.313,10.314-12.715
        c0-4.777-1.808-6.432-4.441-6.432c-0.754,0-1.544,0.113-3.087,1.053l1.807-8.464l-5.571,0.752L54.202,21.48h0
        c-0.564,1.918-1.581,3.31-3.048,3.31c-1.054,0-1.543-0.339-1.543-1.392c0-0.376,0.038-0.789,0.15-1.204l2.711-12.789h-5.42
        l-2.56,12.075c-0.565,1.918-1.543,3.31-3.012,3.31c-0.866,0-1.129-0.677-1.129-1.542c0-0.338,0.038-0.678,0.113-1.054l2.71-12.789
        h-5.42L35.194,21.48C35.006,22.42,34.892,23.248,34.892,23.925z M59.435,22.194l2.07-9.554c0.414-0.527,1.693-0.677,2.221-0.677
        c1.355,0,1.994,1.768,1.994,3.611c0,3.95-1.957,9.216-4.404,9.216c-1.316,0-1.995-0.563-1.995-1.73
        C59.321,22.796,59.359,22.495,59.435,22.194z M18,35h113v-2H18.441L18,35z" />
    </svg>

                            </a>
                        </div>
                    </div>
                </div>
            </div>
            <div class="justifize__box">
                <div class="tableize tableize--full tableize--middle">
                    <div class="tableize__cell">
                        <nav class="header-main__nav">
                            <ul class="list-bare list-inline">
                                <li><a href="/search/london/pop-up-restaurants">Grub Clubs</a></li>
                                <li><a href="/Home/Chefs">Chefs</a></li>
                                <li><a href="/Home/AboutUs">How it Works</a></li>
                                <li><a href="http://grubclub.com/blog">Blog</a></li>
                            </ul>


                        </nav>
                    </div>
                </div>
            </div>
       
            <div class="justifize__box header-main__right">
                <div class="tableize tableize--full tableize--middle">
                    <div class="tableize__cell">
                                          </div>
                </div>
            </div>
        </div>
    </div>



</header>
<!-- End Header -->

     <div class="container pv">
    <div class="layout">
        <div class="layout__item large-and-up-1/2">
            <h1 class="type-x-huge color-orange type-bold mb"><span class='type-super-huge type-uppercase line-height-1'>404</span><br>Oooops! That’s an error!</h1>
            <p class="opacity-7 type-light">The page you are looking for does not exist</p>
        </div>
        <div class="layout__item large-and-up-1/2">
            <div class="pt+">
                <img src="/content/images/404.png" alt="">
            </div>
        </div>
    </div>
</div>


<footer class="footer-main">
    <img src="../Content/images/footer-img.jpg" alt="" class="is-active">
    <div class="container pv">
        <div class="layout">
            <div class="layout__item large-and-up-2/6 pr">
                <h2 class="mb-">About Grub Club</h2>
                <p>This is a story of two food obsessed people who realised they simply wanted to share what they loved doing every day: get people together to share laughter around a dinner table... <a href="">read more</a></p>
            </div>

            <div class="layout__item large-and-up-1/6">
                <h2 class="mb-">Links</h2>
                <ul class="list-bare">
                    <li><a href="/search/london/pop-up-restaurants">Grub Clubs</a></li>
                    <li><a href="/Home/HowItWorks">How It Works</a></li>
                    <li><a href="/Home/Chefs">Chefs</a></li>
                    <li><a href="http://grubclub.com/blog">Blog</a></li>
                    <li><a href="/Home/AboutUs">About Us</a></li>
                    <li><a href="/Home/Faqs">FAQs</a></li>
                    <li><a href="/home/ContactUs">Contact</a></li>
                </ul>
            </div>

            <div class="layout__item large-and-up-1/4">
                <h2>Contact Info</h2>
                <div class="flag flag--top flag--small mb--">
                    <div class="flag__img">

                        <svg role="img" class="icon color-red icon--12x18">
                            <use xlink:href="/Content/images/icons.svg#icon-large_pin"></use>
                        </svg>

                    </div>
                    <div class="flag__body">
                        <p>96 Camden High St, <br>London, NW1 0LQ</p>
                    </div>
                </div>
                <div class="flag flag--top flag--small mb">
                    <div class="flag__img">

                        <svg role="img" class="icon icon--14x11">
                            <use xlink:href="/Content/images/icons.svg#icon-mail_footer"></use>
                        </svg>

                    </div>
                    <div class="flag__body">
                        <p>Email: <a href="mailto:eat@grubclub.com">eat@grubclub.com</a></p>
                    </div>
                </div>
                <h2 class="mb- pt--">Newsletter</h2>
                <form action="" class="footer-main__newsletter">
                    <input type="email" id="newsletter_emailfooter" class="input input--primary" placeholder="Email address">
                   
                </form>
            </div>

            <div class="layout__item large-and-up-1/4">
                <h2 class="mb-">Get Involved</h2>
                <a href="/host/welcome" class="btn btn--primary btn--full btn--h55 btn--radius type-huge mb+">Host a Grub Club punk!</a>
                <h2 class="mb- pt-">Join The Conversation</h2>
                <div class="footer-main__social">
                    <div class="">
                        <div class="justifize__box" style="padding-right:7px;">
                            <a href="http://www.twitter.com/Grub_Club" class="icon-wrap text-center bgr-twitter">
                                <svg role="img" class="icon color-white icon--16x13">
                                    <use xlink:href="/Content/images/icons.svg#icon-twitter"></use>
                                </svg>
                            </a>
                        </div>
                        <div class="justifize__box" style="padding-right:7px;">
                            <a href="http://www.facebook.com/GrubClub1" class="icon-wrap text-center bgr-facebook">
                                <svg role="img" class="icon color-white icon--9x17">
                                    <use xlink:href="/Content/images/icons.svg#icon-facebook"></use>
                                </svg>
                            </a>
                        </div>
                        <div class="justifize__box">
                            <a href="http://instagram.com/grub_club/" class="icon-wrap text-center bgr-instagram">
                                <svg role="img" class="icon color-white icon--15x15">
                                    <use xlink:href="/Content/images/icons.svg#icon-instagram"></use>
                                </svg>
                            </a>
                        </div>
                 
                    </div>
                </div>
            </div>
        </div>
        <a href="" class="to-top-button">
            <div>
                <svg role="img" class="icon icon--29x18">
                    <use xlink:href="/Content/images/icons.svg#icon-arrow_8x14"></use>
                </svg>
            </div>
            <p>Back to Top</p>
        </a>
    </div>
    <div class="footer-main__bottom-bar bgr-orange pv-- color-white mt-">
        <div class="container">
            <div class="justifize">
                <div class="justifize__box">
                    <span class="type-tiny">COPYRIGHT &copy; GrubClub 2013</span>
                </div>
                <div class="justifize__box">
                    <a href="http://grubclub.com/terms-and-conditions" class="color-white type-tiny">Terms &amp; Conditions</a>
                    <span> | </span>
                    <a href="" class="color-white type-tiny">Privacy Policy</a>
                </div>
         
            </div>
        </div>
    </div>

    <div id="info-msg" class="info-msg mfp-hide is-success magnific-content ">
        <div class="text-center">
            <div id="newsletter_ThankyouFooter" class="info-msg__success">Thanks for signing up to the club!</div>
            <div id="newsletter_ErrorFooter" class="info-msg__fail">Please enter your email id</div>
        </div>
    </div>
    

    


    <!-- Back To Top Button -->
    <div id="backtotop"><a href="#"></a></div>

    <!-- Imagebox Build -->
    <script src="/Content/js/imagebox.build.js")" type="text/javascript"></script>

    <!-- Menu Highlighter -->
    
     <!-- Google Analytics -->    
    <script type="text/javascript">

        var _gaq = _gaq || [];
        _gaq.push(['_setAccount', 'UA-36514914-1']);
        _gaq.push(['_trackPageview']);

        (function () {
            var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
            ga.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'stats.g.doubleclick.net/dc.js';
            var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
        })();

    </script>


 <!-- Start Visual Website Optimizer Asynchronous Code -->
    <script type='text/javascript'>
        var _vwo_code = (function () {
            var account_id = 46643,
            settings_tolerance = 2000,
            library_tolerance = 2500,
            use_existing_jquery = false,
            // DO NOT EDIT BELOW THIS LINE
            f = false, d = document; return { use_existing_jquery: function () { return use_existing_jquery; }, library_tolerance: function () { return library_tolerance; }, finish: function () { if (!f) { f = true; var a = d.getElementById('_vis_opt_path_hides'); if (a) a.parentNode.removeChild(a); } }, finished: function () { return f; }, load: function (a) { var b = d.createElement('script'); b.src = a; b.type = 'text/javascript'; b.innerText; b.onerror = function () { _vwo_code.finish(); }; d.getElementsByTagName('head')[0].appendChild(b); }, init: function () { settings_timer = setTimeout('_vwo_code.finish()', settings_tolerance); this.load('//dev.visualwebsiteoptimizer.com/j.php?a=' + account_id + '&u=' + encodeURIComponent(d.URL) + '&r=' + Math.random()); var a = d.createElement('style'), b = 'body{opacity:0 !important;filter:alpha(opacity=0) !important;background:none !important;}', h = d.getElementsByTagName('head')[0]; a.setAttribute('id', '_vis_opt_path_hides'); a.setAttribute('type', 'text/css'); if (a.styleSheet) a.styleSheet.cssText = b; else a.appendChild(d.createTextNode(b)); h.appendChild(a); return settings_timer; } };
        }()); _vwo_settings_timer = _vwo_code.init();
    </script>
    <!-- End Visual Website Optimizer Asynchronous Code -->


    </footer>


    </body>
</html>
