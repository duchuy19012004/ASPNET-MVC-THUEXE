var selectedVehicles = [];
var totalDailyRate = 0;
var allVehicles = [];
var filteredVehicles = [];
$(document).ready(function () {
  // Expect window.XeList to be set in the view as a global JS variable
  if (window.XeList) {
    allVehicles = window.XeList;
  } else {
    allVehicles = [];
  }
  filteredVehicles = [...allVehicles];

  // Initialize
  initializeData();
  initializeSelect2();
  setupEventHandlers();

  // Set tiền cọc mặc định = 0
  $("#TienCoc").val(0);

  // Add live preview for currency formatting
  setupCurrencyPreview();

  // Khởi tạo tính toán tổng kết ban đầu
  calculateTotal();
});

function setupCurrencyPreview() {
  // Tiền cọc preview
  const tienCocInput = document.getElementById("TienCoc");
  if (tienCocInput) {
    const tienCocPreview = document.createElement("small");
    tienCocPreview.className = "form-text text-info currency-preview";
    tienCocInput.parentNode.appendChild(tienCocPreview);

    tienCocInput.addEventListener("input", function (e) {
      const value = parseInt(e.target.value);
      if (value && !isNaN(value) && value > 0) {
        tienCocPreview.innerHTML =
          '<i class="bi bi-eye"></i> Hiển thị: ' +
          value.toLocaleString("vi-VN") +
          " VNĐ";
      } else {
        tienCocPreview.innerHTML = "";
      }
    });
  }

  // Phụ phí preview
  const phuPhiInput = document.getElementById("PhuPhi");
  if (phuPhiInput) {
    const phuPhiPreview = document.createElement("small");
    phuPhiPreview.className = "form-text text-info currency-preview";
    phuPhiInput.parentNode.appendChild(phuPhiPreview);

    phuPhiInput.addEventListener("input", function (e) {
      const value = parseInt(e.target.value);
      if (value && !isNaN(value) && value > 0) {
        phuPhiPreview.innerHTML =
          '<i class="bi bi-eye"></i> Hiển thị: ' +
          value.toLocaleString("vi-VN") +
          " VNĐ";
      } else {
        phuPhiPreview.innerHTML = "";
      }
    });
  }
}

function initializeData() {
  filteredVehicles = [...allVehicles];
  updateFilterOptions();
  updateSearchInfo();
}

function initializeSelect2() {
  // Destroy existing Select2 if any
  if ($("#availableVehicles").hasClass("select2-hidden-accessible")) {
    $("#availableVehicles").select2("destroy");
  }

  $("#availableVehicles").select2({
    placeholder: "Gõ để tìm kiếm xe (tên, biển số, hãng)",
    allowClear: true,
    language: {
      noResults: function () {
        return "Không tìm thấy xe nào";
      },
      searching: function () {
        return "Đang tìm kiếm...";
      },
    },
    templateResult: formatVehicleOption,
    templateSelection: formatVehicleSelection,
  });

  populateVehicleSelect();
}

function formatVehicleOption(vehicle) {
  if (!vehicle.id) return vehicle.text;

  var vehicleData = allVehicles.find((v) => v.maXe == vehicle.id);
  if (!vehicleData) return vehicle.text;
  // Tạo hình ảnh cho option
  var imageHtml = "";
  if (vehicleData.hinhAnh && vehicleData.hinhAnh.trim() !== "") {
    imageHtml = `<img src="/images/xe/${vehicleData.hinhAnh}" alt="${vehicleData.tenXe}" onerror="this.style.display='none'">`;
  } else {
    imageHtml =
      '<div class="vehicle-option-image-placeholder"><i class="bi bi-bicycle"></i></div>';
  }

  // Tạo badge trạng thái
  var statusBadge = "";
  if (vehicleData.trangThai) {
    var statusClass =
      vehicleData.trangThai === "Sẵn sàng"
        ? "badge-success"
        : vehicleData.trangThai === "Đang thuê"
        ? "badge-primary"
        : vehicleData.trangThai === "Bảo trì"
        ? "badge-warning"
        : vehicleData.trangThai === "Hư hỏng"
        ? "badge-danger"
        : vehicleData.trangThai === "Mất"
        ? "badge-dark"
        : "badge-secondary";
    statusBadge = `<span class="badge ${statusClass}">${vehicleData.trangThai}</span>`;
  }

  return $(`
        <div class="select2-vehicle-option">
            ${imageHtml}
            <div class="vehicle-option-info">
                <div class="vehicle-name">${vehicleData.tenXe} - ${
    vehicleData.bienSoXe
  }</div>
                <div class="vehicle-details">${vehicleData.hangXe} ${
    vehicleData.dongXe
  } - ${vehicleData.loaiXe || "Chưa phân loại"}</div>
                <div class="vehicle-status">${statusBadge}</div>
            </div>
            <div class="vehicle-option-price">${vehicleData.giaThue.toLocaleString(
              "vi-VN"
            )}/ngày</div>
        </div>
    `);
}

function formatVehicleSelection(vehicle) {
  if (!vehicle.id) return vehicle.text;

  var vehicleData = allVehicles.find((v) => v.maXe == vehicle.id);
  if (!vehicleData) return vehicle.text;

  return vehicleData.tenXe + " - " + vehicleData.bienSoXe;
}

function setupEventHandlers() {
  // Customer name validation
  $("#HoTenKhach").on("input", function () {
    clearTimeout(window.nameTimeout);
    window.nameTimeout = setTimeout(function () {
      validateName();
    }, 300);
  });

  // CCCD/CMND validation
  $("#SoCCCD").on("input", function () {
    clearTimeout(window.cccdTimeout);
    window.cccdTimeout = setTimeout(function () {
      validateCCCD();
    }, 300);
  });

  // Xử lý paste cho CCCD
  $("#SoCCCD").on("paste", function (e) {
    setTimeout(function () {
      validateCCCD();
    }, 100);
  });

  // Validate Số điện thoại theo thời gian thực
  $("#SoDienThoai").on("input", function () {
    clearTimeout(window.phoneTimeout);
    window.phoneTimeout = setTimeout(function () {
      validatePhone();
    }, 300);
  });

  // Xử lý paste cho số điện thoại
  $("#SoDienThoai").on("paste", function (e) {
    setTimeout(function () {
      validatePhone();
    }, 100);
  });

  // Don't trigger validation on page load - only validate when user interacts
  // or when form is submitted

  // Rental period handlers
  $("#rentalDays").on("input", function () {
    var days = parseInt($(this).val());
    if (days && days > 0) {
      calculateFromDays(days);
      updatePresetButtons();
      calculateTotal(); // Thêm dòng này
    }
  });

  // Khi thay đổi ngày nhận xe, cập nhật lại ngày trả xe dựa trên số ngày
  $("#NgayNhanXe").on("change", function () {
    var days = parseInt($("#rentalDays").val());
    if (days && days > 0) {
      calculateFromDays(days);
    }
    calculateTotal();
  });

  // Khi thay đổi ngày trả xe, cập nhật lại số ngày thuê
  $("#NgayTraXeDuKien").on("change", function () {
    updateRentalDaysFromDates();
    calculateTotal();
  });

  // Thêm xe vào danh sách
  $("#btnAddVehicle").on("click", function () {
    var vehicleId = $("#availableVehicles").val();

    if (!vehicleId) {
      toastr.warning("Vui lòng chọn xe!");
      return;
    }

    // Kiểm tra xe đã được chọn chưa
    if (selectedVehicles.some((v) => v.id == vehicleId)) {
      toastr.warning("Xe này đã được chọn!");
      return;
    }

    // Tìm thông tin xe
    var vehicle = allVehicles.find((v) => v.maXe == vehicleId);
    if (vehicle) {
      // Kiểm tra trạng thái xe
      if (vehicle.trangThai !== "Sẵn sàng") {
        toastr.error(
          `Xe ${vehicle.tenXe} - ${vehicle.bienSoXe} hiện tại có trạng thái "${vehicle.trangThai}" và không thể thuê!`
        );
        return;
      }

      var vehicleObj = {
        id: vehicle.maXe,
        ten: vehicle.tenXe,
        bien: vehicle.bienSoXe,
        hang: vehicle.hangXe,
        dong: vehicle.dongXe,
        gia: vehicle.giaThue,
        loaiXe: vehicle.loaiXe,
        trangThai: vehicle.trangThai,
        display: vehicle.display,
        hinhAnh: vehicle.hinhAnh || "",
      };

      selectedVehicles.push(vehicleObj);
      updateVehiclesList();
      updateSummary();
      calculateTotal();

      // Reset selection và refresh dropdown
      $("#availableVehicles").val(null).trigger("change");
      populateVehicleSelect(); // Refresh để loại bỏ xe đã chọn
    }
  });

  // Xóa xe khỏi danh sách
  $(document).on("click", ".btn-remove-vehicle", function () {
    var vehicleId = $(this).data("vehicle-id");
    selectedVehicles = selectedVehicles.filter((v) => v.id != vehicleId);
    updateVehiclesList();
    updateSummary();
    calculateTotal();
    populateVehicleSelect(); // Refresh để thêm lại xe vào dropdown
  });

  // Filter handlers
  $("#filterHang, #filterLoai, #filterGia").on("change", function () {
    applyFilters();
  });

  $("#btnClearFilters").on("click", function () {
    $("#filterHang, #filterLoai, #filterGia").val("");
    $(".btn-quick-filter").removeClass("active");
    applyFilters();
  });

  // Quick filter buttons
  $(".btn-quick-filter").on("click", function () {
    $(".btn-quick-filter").removeClass("active");
    $(this).addClass("active");

    var filter = $(this).data("filter");
    applyQuickFilter(filter);
  });

  // Khi thay đổi tiền cọc, phụ phí
  $("#TienCoc, #PhuPhi").on("input", function () {
    calculateTotal();
  });

  // Validate form submit
  $("#createContractForm").on("submit", function (e) {
    // Validate xe đã chọn
    if (selectedVehicles.length === 0) {
      e.preventDefault();
      $("#vehicleSelectionError").text("Vui lòng chọn ít nhất một xe!");
      $("html, body").animate(
        {
          scrollTop: $("#vehicleSelectionError").offset().top - 100,
        },
        500
      );
      return false;
    } else {
      $("#vehicleSelectionError").text("");
    }

    // Validate Customer name
    var nameValue = $("#HoTenKhach").val().trim();
    if (nameValue.length === 0) {
      e.preventDefault();
      $("#nameValidationMessage")
        .html(
          '<i class="bi bi-exclamation-triangle"></i> Vui lòng nhập họ tên khách hàng'
        )
        .show();
      $("#nameValidationMessage")
        .removeClass("text-success text-info text-warning")
        .addClass("text-danger");
      $("html, body").animate(
        {
          scrollTop: $("#HoTenKhach").offset().top - 100,
        },
        500
      );
      return false;
    } else if (!validateName()) {
      e.preventDefault();
      $("html, body").animate(
        {
          scrollTop: $("#HoTenKhach").offset().top - 100,
        },
        500
      );
      return false;
    }

    // Validate CCCD/CMND
    var cccdValue = $("#SoCCCD").val();
    if (cccdValue.length === 0) {
      e.preventDefault();
      $("#cccdValidationMessage")
        .html(
          '<i class="bi bi-exclamation-triangle"></i> Vui lòng nhập số CCCD/CMND'
        )
        .show();
      $("#cccdValidationMessage")
        .removeClass("text-success text-info text-warning")
        .addClass("text-danger");
      $("html, body").animate(
        {
          scrollTop: $("#SoCCCD").offset().top - 100,
        },
        500
      );
      return false;
    } else if (!validateCCCD()) {
      e.preventDefault();
      $("html, body").animate(
        {
          scrollTop: $("#SoCCCD").offset().top - 100,
        },
        500
      );
      return false;
    }

    // Validate Số điện thoại
    var phoneValue = $("#SoDienThoai").val();
    if (phoneValue.length === 0) {
      e.preventDefault();
      $("#phoneValidationMessage")
        .html(
          '<i class="bi bi-exclamation-triangle"></i> Vui lòng nhập số điện thoại'
        )
        .show();
      $("#phoneValidationMessage")
        .removeClass("text-success text-info text-warning")
        .addClass("text-danger");
      $("html, body").animate(
        {
          scrollTop: $("#SoDienThoai").offset().top - 100,
        },
        500
      );
      return false;
    } else if (!validatePhone()) {
      e.preventDefault();
      $("html, body").animate(
        {
          scrollTop: $("#SoDienThoai").offset().top - 100,
        },
        500
      );
      return false;
    }

    // Validate required date fields
    var ngayNhan = $("#NgayNhanXe").val();
    var ngayTra = $("#NgayTraXeDuKien").val();

    if (!ngayNhan) {
      e.preventDefault();
      toastr.error("Vui lòng chọn ngày nhận xe!");
      $("html, body").animate(
        {
          scrollTop: $("#NgayNhanXe").offset().top - 100,
        },
        500
      );
      return false;
    }

    if (!ngayTra) {
      e.preventDefault();
      toastr.error("Vui lòng chọn ngày trả xe dự kiến!");
      $("html, body").animate(
        {
          scrollTop: $("#NgayTraXeDuKien").offset().top - 100,
        },
        500
      );
      return false;
    }

    // Validate date logic
    var ngayNhanDate = new Date(ngayNhan);
    var ngayTraDate = new Date(ngayTra);
    var today = new Date();
    today.setHours(0, 0, 0, 0);

    if (ngayNhanDate < today) {
      e.preventDefault();
      toastr.error("Ngày nhận xe không được là ngày trong quá khứ!");
      $("html, body").animate(
        {
          scrollTop: $("#NgayNhanXe").offset().top - 100,
        },
        500
      );
      return false;
    }

    if (ngayTraDate <= ngayNhanDate) {
      e.preventDefault();
      toastr.error("Ngày trả xe phải sau ngày nhận xe!");
      $("html, body").animate(
        {
          scrollTop: $("#NgayTraXeDuKien").offset().top - 100,
        },
        500
      );
      return false;
    }

    // Xóa các input hidden cũ nếu có
    $('input[name^="danhSachXe["]').remove();

    // Thêm danh sách xe vào form
    selectedVehicles.forEach(function (vehicle, index) {
      $("<input>")
        .attr({
          type: "hidden",
          name: "danhSachXe[" + index + "]",
          value: vehicle.id,
        })
        .appendTo("#createContractForm");
    });

    if (typeof $(this).valid === "function" && !$(this).valid()) {
      return false;
    }

    // Check if there are any file uploads
    var hasFileUploads =
      document.getElementById("cccdMatTruoc").files.length > 0 ||
      document.getElementById("cccdMatSau").files.length > 0 ||
      document.getElementById("bangLaiXe").files.length > 0 ||
      document.getElementById("giayToKhac").files.length > 0;

    if (hasFileUploads) {
      // For file uploads, use regular form submission instead of AJAX
      // The form will submit normally and the controller will handle it
      return true; // Allow form to submit normally
    } else {
      // Hiển thị modal xác nhận
      showConfirmModal(selectedVehicles.length);
      return false; // Ngăn form submit ngay lập tức
    }
  });
}

function updateFilterOptions() {
  // Cập nhật filter hãng xe
  var hangs = [...new Set(allVehicles.map((v) => v.hangXe).filter((h) => h))];
  var $filterHang = $("#filterHang");
  $filterHang.empty().append('<option value="">Tất cả hãng</option>');
  hangs.forEach((hang) => {
    $filterHang.append(`<option value="${hang}">${hang}</option>`);
  });

  // Cập nhật filter loại xe
  var loaiXes = [...new Set(allVehicles.map((v) => v.loaiXe).filter((l) => l))];
  var $filterLoai = $("#filterLoai");
  $filterLoai.empty().append('<option value="">Tất cả loại</option>');
  loaiXes.forEach((loai) => {
    $filterLoai.append(`<option value="${loai}">${loai}</option>`);
  });
}

function applyFilters() {
  filteredVehicles = allVehicles.filter((vehicle) => {
    // Filter theo hãng
    var hangFilter = $("#filterHang").val();
    if (hangFilter && vehicle.hangXe !== hangFilter) return false;

    // Filter theo loại xe
    var loaiFilter = $("#filterLoai").val();
    if (loaiFilter && vehicle.loaiXe !== loaiFilter) return false;

    // Filter theo giá
    var giaFilter = $("#filterGia").val();
    if (giaFilter) {
      var [min, max] = giaFilter.split("-").map(Number);
      if (vehicle.giaThue < min || vehicle.giaThue > max) return false;
    }

    return true;
  });

  populateVehicleSelect();
  updateSearchInfo();
}

function applyQuickFilter(filter) {
  $("#filterHang, #filterLoai, #filterGia").val("");

  switch (filter) {
    case "honda":
      $("#filterHang").val("Honda");
      break;
    case "yamaha":
      $("#filterHang").val("Yamaha");
      break;
    case "suzuki":
      $("#filterHang").val("Suzuki");
      break;
    case "cheap":
      $("#filterGia").val("0-150000");
      break;
    case "premium":
      $("#filterGia").val("300000-999999999");
      break;
    case "xe-may":
      $("#filterLoai").val("Xe máy");
      break;
    case "xe-so":
      $("#filterLoai").val("Xe số");
      break;
  }

  applyFilters();
}

function populateVehicleSelect() {
  var $select = $("#availableVehicles");

  // Xóa tất cả options trừ placeholder
  $select.find("option:not(:first)").remove();

  // Lọc xe chưa được chọn và chỉ hiển thị xe có trạng thái "Sẵn sàng"
  var availableVehicles = filteredVehicles.filter(
    (vehicle) =>
      !selectedVehicles.some((selected) => selected.id == vehicle.maXe) &&
      vehicle.trangThai === "Sẵn sàng"
  );

  // Thêm options mới với hình ảnh
  availableVehicles.forEach((vehicle) => {
    var option = new Option(vehicle.display, vehicle.maXe);
    $select.append(option);
  });

  // Refresh Select2
  $select.trigger("change");
}

function updateSearchInfo() {
  var availableCount = filteredVehicles.filter(
    (vehicle) =>
      !selectedVehicles.some((selected) => selected.id == vehicle.maXe) &&
      vehicle.trangThai === "Sẵn sàng"
  ).length;

  $("#availableCount").text(availableCount);

  if (availableCount === 0 && filteredVehicles.length === 0) {
    $("#searchInfo").html(
      '<i class="bi bi-exclamation-triangle"></i> Không có xe nào khớp với bộ lọc. Vui lòng thử lại.'
    );
  } else if (availableCount === 0) {
    $("#searchInfo").html(
      '<i class="bi bi-check-circle"></i> Tất cả xe sẵn sàng đã được chọn hoặc không có xe nào sẵn sàng.'
    );
  } else {
    $("#searchInfo").html(
      `Hiển thị <span id="availableCount">${availableCount}</span> xe sẵn sàng. Sử dụng tìm kiếm hoặc bộ lọc để tìm xe nhanh hơn.`
    );
  }
}

function updateVehiclesList() {
  var container = $("#selectedVehiclesList");

  if (selectedVehicles.length === 0) {
    container.html(
      '<div class="no-vehicles-selected" id="noVehiclesMessage"><i class="bi bi-info-circle"></i> Chưa có xe nào được chọn</div>'
    );
    $("#vehiclesSummary").hide();
    return;
  }

  var html = "";
  selectedVehicles.forEach(function (vehicle) {
    // Tạo hình ảnh xe
    var imageHtml = "";
    if (vehicle.hinhAnh && vehicle.hinhAnh.trim() !== "") {
      imageHtml = `<img src="/images/xe/${vehicle.hinhAnh}" alt="${vehicle.ten}" class="vehicle-image" onerror="this.parentElement.innerHTML='<div class=\'vehicle-image-placeholder\'><i class=\'bi bi-bicycle\'></i></div>'">`;
    } else {
      imageHtml =
        '<div class="vehicle-image-placeholder"><i class="bi bi-bicycle"></i></div>';
    }

    html += `
            <div class="vehicle-item">
                ${imageHtml}
                <div class="vehicle-info-display">
                    <h6>${vehicle.ten} - ${vehicle.bien}</h6>
                    <small>${vehicle.hang} ${
      vehicle.dong
    } - <span class="text-primary">${
      vehicle.loaiXe || "Chưa phân loại"
    }</span></small>
                </div>
                <div class="vehicle-price">
                    ${vehicle.gia.toLocaleString("vi-VN")}/ngày
                </div>
                <button type="button" class="btn btn-danger btn-remove-vehicle" data-vehicle-id="${
                  vehicle.id
                }">
                    <i class="bi bi-trash"></i> Xóa
                </button>
            </div>
        `;
  });

  container.html(html);
  $("#vehiclesSummary").show();
  updateSearchInfo(); // Cập nhật số lượng xe available
}

function updateSummary() {
  totalDailyRate = selectedVehicles.reduce(
    (sum, vehicle) => sum + vehicle.gia,
    0
  );

  $("#totalVehicles").text(selectedVehicles.length);
  $("#totalDailyPrice").text(
    totalDailyRate === 0 ? "0đ" : totalDailyRate.toLocaleString("vi-VN")
  );
  $("#TotalDailyRateDisplay").val(
    totalDailyRate === 0 ? "0đ" : totalDailyRate.toLocaleString("vi-VN")
  );
}

function calculateTotal() {
  var ngayNhan = new Date($("#NgayNhanXe").val());
  var ngayTra = new Date($("#NgayTraXeDuKien").val());
  var tienCoc = parseFloat($("#TienCoc").val()) || 0;
  var phuPhi = parseFloat($("#PhuPhi").val()) || 0;

  if (ngayNhan && ngayTra && ngayTra > ngayNhan && totalDailyRate > 0) {
    var soNgay = Math.ceil((ngayTra - ngayNhan) / (1000 * 60 * 60 * 24));
    var tienThue = soNgay * totalDailyRate;
    var tongTien = tienThue + tienCoc + phuPhi;

    // Cập nhật hiển thị với định dạng VN
    $("#soXeThue").text(selectedVehicles.length);
    $("#soNgayThue").text(soNgay);
    $("#tienThue").html(
      tienThue === 0 ? "0" : tienThue.toLocaleString("vi-VN") + "đ"
    );
    $("#tienCocDisplay").html(
      tienCoc === 0 ? "0" : tienCoc.toLocaleString("vi-VN") + "đ"
    );
    $("#phuPhiDisplay").html(
      phuPhi === 0 ? "0" : phuPhi.toLocaleString("vi-VN") + "đ"
    );
    $("#tongTien").html(
      tongTien === 0 ? "0" : tongTien.toLocaleString("vi-VN") + "đ"
    );
    $("#TongTienInput").val(tongTien);
  } else {
    // Reset về 0 với định dạng
    $("#soXeThue").text(selectedVehicles.length);
    $("#soNgayThue").text("0");
    $("#tienThue").html("0đ");
    $("#tienCocDisplay").html("0đ");
    $("#phuPhiDisplay").html("0đ");
    $("#tongTien").html("0đ");
    $("#TongTienInput").val(0);
  }
}

// Rental period functions
function calculateFromDays(days) {
  var ngayNhan = $("#NgayNhanXe").val();
  if (!ngayNhan) {
    var today = new Date();
    ngayNhan = today.toISOString().split("T")[0];
    $("#NgayNhanXe").val(ngayNhan);
  }

  var startDate = new Date(ngayNhan);
  var endDate = new Date(startDate);
  endDate.setDate(startDate.getDate() + days);

  var endDateStr = endDate.toISOString().split("T")[0];
  $("#NgayTraXeDuKien").val(endDateStr);
}

function setRentalDays(days) {
  $("#rentalDays").val(days);
  calculateFromDays(days);
  updatePresetButtons();
  calculateTotal(); // Thêm dòng này
}

function updatePresetButtons() {
  var currentDays = parseInt($("#rentalDays").val());
  $(".btn-preset").removeClass("active");

  if (currentDays) {
    $(".btn-preset").each(function () {
      var buttonDays = parseInt($(this).attr("onclick").match(/\d+/)[0]);
      if (buttonDays === currentDays) {
        $(this).addClass("active");
      }
    });
  }
}

function updateRentalDaysFromDates() {
  var ngayNhan = new Date($("#NgayNhanXe").val());
  var ngayTra = new Date($("#NgayTraXeDuKien").val());

  if (ngayNhan && ngayTra && ngayTra >= ngayNhan) {
    var days = Math.ceil((ngayTra - ngayNhan) / (1000 * 60 * 60 * 24));
    $("#rentalDays").val(days);
    updatePresetButtons();
  }
}

// Customer name validation function
function validateName() {
  var nameValue = $("#HoTenKhach").val().trim();
  var validationMessage = $("#nameValidationMessage");

  // Kiểm tra độ dài
  if (nameValue.length === 0) {
    validationMessage.html("").hide();
    validationMessage.removeClass(
      "text-danger text-success text-warning text-info"
    );
    return false;
  } else if (nameValue.length < 3) {
    validationMessage
      .html(
        '<i class="bi bi-exclamation-triangle"></i> Họ tên phải có ít nhất 3 ký tự'
      )
      .show();
    validationMessage
      .removeClass("text-success text-info text-danger")
      .addClass("text-warning");
    return false;
  } else if (nameValue.length > 100) {
    validationMessage
      .html(
        '<i class="bi bi-exclamation-triangle"></i> Họ tên không được vượt quá 100 ký tự'
      )
      .show();
    validationMessage
      .removeClass("text-success text-info text-warning")
      .addClass("text-danger");
    return false;
  } else {
    // Kiểm tra format: phải có ít nhất 2 từ (họ và tên)
    var words = nameValue.split(" ").filter((word) => word.length > 0);
    if (words.length < 2) {
      validationMessage
        .html(
          '<i class="bi bi-exclamation-triangle"></i> Vui lòng nhập đầy đủ họ và tên'
        )
        .show();
      validationMessage
        .removeClass("text-success text-info text-warning")
        .addClass("text-warning");
      return false;
    }

    validationMessage
      .html('<i class="bi bi-check-circle"></i> Họ tên hợp lệ')
      .show();
    validationMessage
      .removeClass("text-warning text-info text-danger")
      .addClass("text-success");
    return true;
  }
}

// CCCD/CMND validation function
function validateCCCD() {
  var cccdValue = $("#SoCCCD").val();
  var validationMessage = $("#cccdValidationMessage");

  // Loại bỏ tất cả ký tự không phải số
  var numericOnly = cccdValue.replace(/[^0-9]/g, "");

  // Cập nhật giá trị input nếu có ký tự không phải số
  if (cccdValue !== numericOnly) {
    $("#SoCCCD").val(numericOnly);
    cccdValue = numericOnly;
  }

  // Kiểm tra độ dài
  if (cccdValue.length === 0) {
    validationMessage.html("").hide();
    validationMessage.removeClass(
      "text-danger text-success text-warning text-info"
    );
    return false;
  } else if (cccdValue.length < 12) {
    validationMessage
      .html(
        '<i class="bi bi-exclamation-triangle"></i> CCCD/CMND phải có đúng 12 số (còn thiếu <strong>' +
          (12 - cccdValue.length) +
          "</strong> số)"
      )
      .show();
    validationMessage
      .removeClass("text-success text-info text-danger")
      .addClass("text-warning");
    return false;
  } else if (cccdValue.length > 12) {
    validationMessage
      .html(
        '<i class="bi bi-exclamation-triangle"></i> CCCD/CMND chỉ được nhập tối đa 12 số'
      )
      .show();
    validationMessage
      .removeClass("text-success text-info text-warning")
      .addClass("text-danger");
    return false;
  } else {
    // Kiểm tra thêm: CCCD không được bắt đầu bằng 000000
    if (cccdValue.startsWith("000000")) {
      validationMessage
        .html(
          '<i class="bi bi-exclamation-triangle"></i> CCCD/CMND không hợp lệ (số không được bắt đầu bằng 000000)'
        )
        .show();
      validationMessage
        .removeClass("text-success text-info text-warning")
        .addClass("text-danger");
      return false;
    }

    validationMessage
      .html('<i class="bi bi-check-circle"></i> CCCD/CMND hợp lệ')
      .show();
    validationMessage
      .removeClass("text-warning text-info text-danger")
      .addClass("text-success");
    return true;
  }
}

// Validate Số điện thoại theo thời gian thực
function validatePhone() {
  var phoneValue = $("#SoDienThoai").val();
  var validationMessage = $("#phoneValidationMessage");

  // Loại bỏ ký tự không phải số
  var numericOnly = phoneValue.replace(/[^0-9]/g, "");
  if (phoneValue !== numericOnly) {
    $("#SoDienThoai").val(numericOnly);
    phoneValue = numericOnly;
  }

  // Kiểm tra độ dài
  if (phoneValue.length === 0) {
    validationMessage.html("").hide();
    validationMessage.removeClass(
      "text-danger text-success text-warning text-info"
    );
    return false;
  } else if (phoneValue.length < 10) {
    validationMessage
      .html(
        '<i class="bi bi-exclamation-triangle"></i> Số điện thoại phải có từ 10 đến 11 số (thiếu <strong>' +
          (10 - phoneValue.length) +
          "</strong> số)"
      )
      .show();
    validationMessage
      .removeClass("text-success text-info text-danger")
      .addClass("text-warning");
    return false;
  } else if (phoneValue.length > 11) {
    validationMessage
      .html(
        '<i class="bi bi-exclamation-triangle"></i> Số điện thoại chỉ được nhập tối đa 11 số'
      )
      .show();
    validationMessage
      .removeClass("text-success text-info text-warning")
      .addClass("text-danger");
    return false;
  } else {
    validationMessage
      .html('<i class="bi bi-check-circle"></i> Số điện thoại hợp lệ')
      .show();
    validationMessage
      .removeClass("text-warning text-info text-danger")
      .addClass("text-success");
    return true;
  }
}

// Modal xác nhận tạo hợp đồng
function showConfirmModal(vehicleCount) {
  // Tạo modal HTML nếu chưa có
  if ($("#confirmModal").length === 0) {
    // Thêm CSS cho modal
    if (!$("#confirmModalStyles").length) {
      $("head").append(`
        <style id="confirmModalStyles">
          .confirm-modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            display: none;
            z-index: 9999;
            align-items: center;
            justify-content: center;
          }
          
          .confirm-modal-container {
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
            max-width: 500px;
            width: 90%;
            max-height: 80vh;
            overflow-y: auto;
          }
          
          .confirm-modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 20px;
            border-bottom: 1px solid #e0e0e0;
          }
          
          .confirm-modal-header h4 {
            margin: 0;
            color: #333;
          }
          
          .confirm-modal-close {
            background: none;
            border: none;
            font-size: 1.2rem;
            cursor: pointer;
            color: #666;
            padding: 5px;
          }
          
          .confirm-modal-close:hover {
            color: #333;
          }
          
          .confirm-modal-body {
            padding: 20px;
          }
          
          .confirm-modal-body p {
            margin-bottom: 20px;
            color: #333;
          }
          
          .confirm-modal-details {
            background: #f8f9fa;
            border-radius: 6px;
            padding: 15px;
            margin-bottom: 20px;
          }
          
          .detail-item {
            display: flex;
            justify-content: space-between;
            margin-bottom: 10px;
            padding: 5px 0;
          }
          
          .detail-item:last-child {
            margin-bottom: 0;
          }
          
          .detail-label {
            font-weight: 600;
            color: #333;
          }
          
          .detail-value {
            color: #666;
          }
          
          .confirm-modal-footer {
            display: flex;
            gap: 10px;
            padding: 20px;
            border-top: 1px solid #e0e0e0;
            justify-content: flex-end;
          }
          
          .confirm-modal-footer .btn {
            min-width: 120px;
          }
          
          @media (max-width: 768px) {
            .confirm-modal-container {
              width: 95%;
              margin: 20px;
            }
            
            .confirm-modal-footer {
              flex-direction: column;
            }
            
            .confirm-modal-footer .btn {
              width: 100%;
            }
          }
        </style>
      `);
    }

    $("body").append(`
            <div id="confirmModal" class="confirm-modal-overlay">
                <div class="confirm-modal-container">
                    <div class="confirm-modal-header">
                        <h4><i class="bi bi-question-circle"></i> Xác nhận tạo hợp đồng</h4>
                        <button type="button" class="confirm-modal-close" onclick="hideConfirmModal()">
                            <i class="bi bi-x-lg"></i>
                        </button>
                    </div>
                    <div class="confirm-modal-body">
                        <p>Bạn có chắc chắn muốn tạo hợp đồng này với <strong id="vehicleCountText">${vehicleCount}</strong> xe?</p>
                        <div class="confirm-modal-details">
                            <div class="detail-item">
                                <span class="detail-label">Khách hàng:</span>
                                <span class="detail-value" id="customerNameText"></span>
                            </div>
                            <div class="detail-item">
                                <span class="detail-label">Số điện thoại:</span>
                                <span class="detail-value" id="customerPhoneText"></span>
                            </div>
                            <div class="detail-item">
                                <span class="detail-label">Ngày thuê:</span>
                                <span class="detail-value" id="rentalDateText"></span>
                            </div>
                            <div class="detail-item">
                                <span class="detail-label">Tổng tiền:</span>
                                <span class="detail-value" id="totalAmountText"></span>
                            </div>
                        </div>
                    </div>
                    <div class="confirm-modal-footer">
                        <button type="button" class="btn btn-secondary" onclick="hideConfirmModal()">
                            <i class="bi bi-x-circle"></i> Hủy bỏ
                        </button>
                        <button type="button" class="btn btn-success" onclick="confirmCreateContract()">
                            <i class="bi bi-check-circle"></i> Xác nhận tạo
                        </button>
                    </div>
                </div>
            </div>
        `);
  }

  // Cập nhật thông tin trong modal
  $("#vehicleCountText").text(vehicleCount);
  $("#customerNameText").text($("#HoTenKhach").val());
  $("#customerPhoneText").text($("#SoDienThoai").val());

  var ngayNhan = new Date($("#NgayNhanXe").val());
  var ngayTra = new Date($("#NgayTraXeDuKien").val());
  var soNgay = Math.ceil((ngayTra - ngayNhan) / (1000 * 60 * 60 * 24));
  $("#rentalDateText").text(
    ngayNhan.toLocaleDateString("vi-VN") +
      " - " +
      ngayTra.toLocaleDateString("vi-VN") +
      ` (${soNgay} ngày)`
  );
  var tongTien = parseFloat($("#TongTienInput").val()) || 0;
  $("#totalAmountText").text(
    tongTien === 0 ? "0đ" : tongTien.toLocaleString("vi-VN")
  );

  // Hiển thị modal
  $("#confirmModal").css("display", "flex").fadeIn(300);
}

function hideConfirmModal() {
  $("#confirmModal").fadeOut(300);
}

function validateForm() {
  console.log("DEBUG: Validating form...");

  // Check if vehicles are selected (only required validation)
  var selectedVehicles = window.selectedVehicles || [];
  if (selectedVehicles.length === 0) {
    toastr.error("Vui lòng chọn ít nhất một xe!");
    return false;
  }

  console.log("DEBUG: Form validation passed - no validation for file uploads");
  return true;
}

function confirmCreateContract() {
  console.log("=== DEBUG: Starting contract creation ===");

  // Validate form data
  if (!validateForm()) {
    console.log("DEBUG: Form validation failed");
    return;
  }

  // Create FormData
  var formData = new FormData();

  // Add anti-forgery token
  var token = $('input[name="__RequestVerificationToken"]').val();
  formData.append("__RequestVerificationToken", token);

  // Add form fields
  formData.append("HoTenKhach", document.getElementById("HoTenKhach").value);
  formData.append("SoDienThoai", document.getElementById("SoDienThoai").value);
  formData.append("SoCCCD", document.getElementById("SoCCCD").value);
  formData.append("DiaChi", document.getElementById("DiaChi").value);
  formData.append("NgayNhanXe", document.getElementById("NgayNhanXe").value);
  formData.append(
    "NgayTraXeDuKien",
    document.getElementById("NgayTraXeDuKien").value
  );
  formData.append("TienCoc", document.getElementById("TienCoc").value);
  formData.append("PhuPhi", document.getElementById("PhuPhi").value);
  formData.append("GhiChu", document.getElementById("GhiChu").value);
  formData.append("TongTien", document.getElementById("TongTienInput").value);

  // Add file uploads
  var cccdMatTruocFile = document.getElementById("cccdMatTruoc").files[0];
  if (cccdMatTruocFile) {
    formData.append("cccdMatTruoc", cccdMatTruocFile);
  }

  var cccdMatSauFile = document.getElementById("cccdMatSau").files[0];
  if (cccdMatSauFile) {
    formData.append("cccdMatSau", cccdMatSauFile);
  }

  var bangLaiXeFile = document.getElementById("bangLaiXe").files[0];
  if (bangLaiXeFile) {
    formData.append("bangLaiXe", bangLaiXeFile);
  }

  var giayToKhacFile = document.getElementById("giayToKhac").files[0];
  if (giayToKhacFile) {
    formData.append("giayToKhac", giayToKhacFile);
  }

  console.log("DEBUG: Form fields added to FormData");

  // Add selected vehicles
  var selectedVehicles = window.selectedVehicles || [];
  selectedVehicles.forEach(function (vehicle, index) {
    formData.append("danhSachXe", vehicle.id);
  });

  console.log("DEBUG: Selected vehicles:", selectedVehicles.length);

  // Send AJAX request
  $.ajax({
    url: $("#createContractForm").attr("action"),
    type: "POST",
    data: formData,
    processData: false,
    contentType: false,
    success: function (response) {
      console.log("DEBUG: Success response:", response);
      hideConfirmModal();

      // Check if response is HTML instead of JSON
      if (
        typeof response === "string" &&
        response.includes("<!DOCTYPE html>")
      ) {
        console.log("DEBUG: Server returned HTML instead of JSON");
        toastr.error("Có lỗi xảy ra: Server trả về HTML thay vì JSON");
        return;
      }

      if (response.success) {
        toastr.success("Tạo hợp đồng thành công!");
        setTimeout(function () {
          // Chuyển đến trang chi tiết hợp đồng với tham số mới tạo
          if (response.redirectUrl) {
            window.location.href = response.redirectUrl + "?newlyCreated=true";
          } else {
            window.location.href = "/QuanLyHopDong/Index";
          }
        }, 1500);
      } else {
        toastr.error(
          "Có lỗi xảy ra: " + (response.message || "Không xác định")
        );
      }
    },
    error: function (xhr, status, error) {
      console.log("DEBUG: Error status:", xhr.status);
      console.log("DEBUG: Error response:", xhr.responseText);
      console.log("DEBUG: Error details:", error);
      hideConfirmModal();

      // Try to parse error response for better error message
      try {
        var errorResponse = JSON.parse(xhr.responseText);
        if (errorResponse.message) {
          toastr.error("Có lỗi xảy ra: " + errorResponse.message);
        } else {
          toastr.error("Có lỗi xảy ra khi tạo hợp đồng. Vui lòng thử lại.");
        }
      } catch (e) {
        toastr.error("Có lỗi xảy ra khi tạo hợp đồng. Vui lòng thử lại.");
      }
    },
  });
}
