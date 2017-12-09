var site = {
    insertText: function (obj, str) {
        if (document.selection) {
            var sel = document.selection.createRange();
            sel.text = str;
        } else if (typeof obj.selectionStart === 'number' && typeof obj.selectionEnd === 'number') {
            var startPos = obj.selectionStart,
                endPos = obj.selectionEnd,
                cursorPos = startPos,
                tmpStr = obj.value;
            obj.value = tmpStr.substring(0, startPos) + str + tmpStr.substring(endPos, tmpStr.length);
            cursorPos += str.length;
            obj.selectionStart = obj.selectionEnd = cursorPos;
        } else {
            obj.value += str;
        }
    },
    handleUpload: function () {
        $("#upload").click(function (evt) {
            $("#files").click();
        });
        $("#files").change(function () {
            var type = $(this).data("type")
            var fileUpload = $("#files").get(0);
            var files = fileUpload.files;
            if (fileUpload.files.length > 0) {
                var data = new FormData();
                for (var i = 0; i < files.length; i++) {
                    data.append(files[i].name, files[i]);
                }
                data.append('type', type);
                $.ajax({
                    type: "POST",
                    url: "/UploadFiles",
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (rst) {
                        console.log(rst.message)
                        if ($("#Content").length > 0 || $("#comment_Content").length > 0) {
                            var append = "";
                            $(rst.fileResults).each(function (i, d) {
                                append += '\n ![](' + d + ')';
                            })
                            var $textarea = $("#Content").length > 0 ? $("#Content") : $("#comment_Content")
                            site.insertText($textarea[0], append)
                        }
                        //gallery
                        if (type.indexOf("gallery") != -1) {
                            window.location.reload();
                        }
                        //end gallery
                    },
                    error: function () {
                        alert("There was error uploading files!");
                    }
                });
            }
        })
    },
    handleNavToggle: function () {
        $(".head-nav-toggle > button").click(function () {
            $(".head-nav-list").slideToggle("fast");
        })
    },
    handleBackToTop: function () {
        var THRESHOLD = 200;
        var $top = $('.back-to-top');

        $(window).on('scroll', function () {
            $top.toggleClass('back-to-top-on', window.pageYOffset > THRESHOLD);

            var scrollTop = $(window).scrollTop();
            var docHeight = $('#content').height();
            var winHeight = $(window).height();
            var scrollPercent = (scrollTop) / (docHeight - winHeight);
            var scrollPercentRounded = Math.round(scrollPercent * 100);
            var scrollPercentMaxed = (scrollPercentRounded > 100) ? 100 : scrollPercentRounded;
            $('#scrollpercent>span').html(scrollPercentMaxed);
        });

        $top.on('click', function () {
            $('html,body').animate({ scrollTop: 0 }, 'slow');
        });
    },
    handleFaccybox: function (selector) {
        if (!!$().fancybox) {
            $("[data-fancybox]").fancybox({
                protect: true
            });
        }
    },
    handleLazyload: function () {
        if (!!$().lazyload) {
            $("img.lazy").lazyload({
                effect: "fadeIn"
            });
        }
    },
    handleCodePre: function () {
        $("pre").each(function () {
            $(this).addClass("line-numbers");
        })
    },
    handleMarkdown: function () {
        if ($("#Content").length > 0)
        {
            var rendererMD = new marked.Renderer();
            marked.setOptions({
                renderer: rendererMD,
                gfm: true,
                tables: true,
                breaks: true,
                pedantic: false,
                sanitize: false,
                smartLists: true,
                smartypants: false
            });
            $("#Content").on("keyup blur", function () {
                $("#html-preview").html(marked($("#Content").val()))
            })
            $("#Content").keyup(); 
        }
    },
    handleForm: function (formid, dataType, successfunc) {
        formid = "#" + formid;
        if ($(formid).length > 0) {
            $(formid).submit(function () {
                var url = $(this).attr("action")
                var data = $(this).serialize()
                $.ajax({
                    type: "POST",
                    dataType: dataType,
                    url: url,
                    data: data,
                    success: successfunc,
                    error: function (data) {
                        console.log("error:" + data.responseText);
                    }
                });
                return false;
            })
        }
    },
    handlePostForm: function () {
        site.handleForm("post-form", "json", function (rst) {
            alert(rst.message)
            if (rst.isSuccess == true) {
                window.location = "/Admin/PostEdit/" + rst.data.id
            }
        })

        $(".post-remove").click(function () {
            var $self = $(this)
            if (confirm("are you sure you want to delete this post?")) {
                var id = $self.data("id")
                $.post("/Admin/PostDelete?postId=" + id).done(function (rst) {
                    window.location = "/Admin/PostEdit"
                })
            }
        })
    },
    handleGalleryForm: function () {
        site.handleForm("gallery-form", "json", function (rst) {
            alert(rst.message)
            if (rst.isSuccess == true) {
                window.location = "/Admin/GalleryEdit/" + rst.data.id
            }
        })

        $(".gallery-remove").click(function () {
            var $self = $(this)
            if (confirm("are you sure you want to delete this gallery?")) {
                var id = $self.data("id")
                $.post("/Admin/GalleryDelete?galleryId=" + id).done(function (rst) {
                    window.location = "/Admin/GalleryEdit"
                })
            }
        })
        $(".photo-remove").click(function () {
            var $self = $(this)
            if (confirm("are you sure you want to delete this photo?")) {
                var photoId = $self.data("photo")
                $.post("/Admin/PhotoDelete?photoId=" + photoId).done(function (rst) {
                    $self.parent().remove();
                })
            }
        })
    },
    handleCommentForm: function () {
        site.handleForm("comments-form", "html", function (rst) {
            $("#comments-tips").remove();
            var $comments = $("#comments-list")
            $('html,body')
                .animate({
                    scrollTop: $comments.offset().top - 200
                }, 800)
                .promise()
                .done(function () {
                    var $item = $(rst).hide()
                    $comments.prepend($item)
                    $item.fadeIn();
                });
        })

        $(".comment-remove").click(function () {
            var $self = $(this)
            if (confirm("are you sure you want to delete this comment?")) {
                var id = $self.data("id")
                $.post("/Admin/CommentDelete?commentId=" + id).done(function (rst) {
                    $self.parent().parent().remove();
                })
            }
        })

        $(".comment-approve").click(function () {
            var $self = $(this)
            if (confirm("are you sure you want to approve this comment?")) {
                var id = $self.data("id")
                $.post("/Admin/CommentApproved?commentId=" + id).done(function (rst) {
                    $self.remove();
                })
            }
        })
    },
    handleCommentList: function () {
        if ($("#CommentSourceID").length > 0)
        {
            $(window).bind("scroll", function () {
                var scrollTop = $(this).scrollTop();
                var scrollHeight = $(document).height();
                var windowHeight = $(this).height();
                if (scrollTop + windowHeight == scrollHeight) {
                    $(window).unbind("scroll")
                    site.ajaxLoadCommentList(1)
                }
            })

            $("#post-comments").delegate("a.page-number", "click", function () {
                var pageIndex = $(this).attr("href").replace("#/", "")
                site.ajaxLoadCommentList(pageIndex);
                return false;
            })
        }
    },
    ajaxLoadCommentList: function (pageIndex) {
        var sourceId = $("#CommentSourceID").val()
        var url = "/comments/" + sourceId + "/" + pageIndex
        $.ajax({
            type: "POST",
            dataType: "html",
            url: url,
            success: function (html) {
                $("#post-comments").html(html);
            },
            error: function (data) {
                console.log("error:" + data.responseText);
            }
        });
    }
}

$(function () {
    site.handleUpload();
    site.handleNavToggle();
    site.handleBackToTop();
    site.handleFaccybox();
    site.handleLazyload();
    site.handleCodePre();
    site.handleMarkdown();
    site.handlePostForm();
    site.handleGalleryForm();
    site.handleCommentForm();
    site.handleCommentList();
})