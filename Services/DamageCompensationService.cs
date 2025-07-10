using bike.Models;

namespace bike.Services
{
    public interface IDamageCompensationService
    {
        decimal CalculateCompensation(string damageType, decimal vehicleValue, decimal repairCost = 0);
        string DetermineBikeStatusAfterDamage(string damageType);
        bool ShouldHideBikeFromListing(string bikeStatus);
        CompensationResult ProcessDamageReport(string damageType, decimal vehicleValue, decimal estimatedRepairCost, int rentalDays);
    }

    public class DamageCompensationService : IDamageCompensationService
    {
        // Tính toán phí đền bù dựa trên loại thiệt hại
        public decimal CalculateCompensation(string damageType, decimal vehicleValue, decimal repairCost = 0)
        {
            return damageType switch
            {
                "Bình thường" => 0, // Không có thiệt hại
                "Hư hỏng nhẹ" => Math.Min(repairCost * 1.2m, vehicleValue * 0.15m), // 120% chi phí sửa chữa hoặc 15% giá trị xe
                "Hư hỏng nặng" => Math.Min(repairCost * 1.3m, vehicleValue * 0.6m), // 130% chi phí sửa chữa hoặc 60% giá trị xe
                "Mất" => vehicleValue, // 100% giá trị xe
                "Tai nạn" => Math.Min(repairCost * 1.5m, vehicleValue * 0.8m), // 150% chi phí sửa chữa hoặc 80% giá trị xe
                _ => 0
            };
        }

        // Xác định trạng thái xe sau khi có thiệt hại
        public string DetermineBikeStatusAfterDamage(string damageType)
        {
            return damageType switch
            {
                "Bình thường" => "Sẵn sàng",
                "Hư hỏng nhẹ" => "Bảo trì", // Có thể sửa chữa và cho thuê lại
                "Hư hỏng nặng" => "Hư hỏng", // Cần sửa chữa lớn, tạm thời không cho thuê
                "Mất" => "Mất", // Không còn tồn tại
                "Tai nạn" => "Hư hỏng", // Cần đánh giá và sửa chữa
                _ => "Bảo trì"
            };
        }

        // Kiểm tra xe có nên ẩn khỏi danh sách cho thuê không
        public bool ShouldHideBikeFromListing(string bikeStatus)
        {
            return bikeStatus == "Mất" || bikeStatus == "Hư hỏng";
        }

        // Xử lý báo cáo thiệt hại hoàn chỉnh
        public CompensationResult ProcessDamageReport(string damageType, decimal vehicleValue, decimal estimatedRepairCost, int rentalDays)
        {
            var result = new CompensationResult
            {
                DamageType = damageType,
                VehicleValue = vehicleValue,
                EstimatedRepairCost = estimatedRepairCost,
                RentalDays = rentalDays
            };

            // Tính phí đền bù
            result.CompensationAmount = CalculateCompensation(damageType, vehicleValue, estimatedRepairCost);

            // Xác định trạng thái xe
            result.NewBikeStatus = DetermineBikeStatusAfterDamage(damageType);

            // Kiểm tra có ẩn xe không
            result.ShouldHideFromListing = ShouldHideBikeFromListing(result.NewBikeStatus);

            // Tính toán thêm
            result.CompensationPercentage = vehicleValue > 0 ? (result.CompensationAmount / vehicleValue) * 100 : 0;
            
            // Xác định mức độ nghiêm trọng
            result.SeverityLevel = damageType switch
            {
                "Bình thường" => "Không có",
                "Hư hỏng nhẹ" => "Nhẹ",
                "Hư hỏng nặng" => "Nặng",
                "Mất" => "Rất nặng",
                "Tai nạn" => "Nặng",
                _ => "Không xác định"
            };

            // Gợi ý xử lý
            result.ProcessingSuggestion = damageType switch
            {
                "Bình thường" => "Xe có thể tiếp tục cho thuê ngay",
                "Hư hỏng nhẹ" => "Sửa chữa nhỏ, có thể cho thuê lại sau 1-2 ngày",
                "Hư hỏng nặng" => "Cần sửa chữa toàn diện, tạm ngừng cho thuê",
                "Mất" => "Xe đã mất, cần làm thủ tục bảo hiểm và đền bù",
                "Tai nạn" => "Cần đánh giá kỹ thuật và sửa chữa",
                _ => "Cần đánh giá thêm"
            };

            return result;
        }
    }

    // Model kết quả xử lý đền bù
    public class CompensationResult
    {
        public string DamageType { get; set; } = "";
        public decimal VehicleValue { get; set; }
        public decimal EstimatedRepairCost { get; set; }
        public int RentalDays { get; set; }
        public decimal CompensationAmount { get; set; }
        public string NewBikeStatus { get; set; } = "";
        public bool ShouldHideFromListing { get; set; }
        public decimal CompensationPercentage { get; set; }
        public string SeverityLevel { get; set; } = "";
        public string ProcessingSuggestion { get; set; } = "";
    }
} 