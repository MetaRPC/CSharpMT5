using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MetaRPC.CSharpMT5;
using mt5_term_api;

namespace MetaRPC.CSharpMT5.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly MT5Account _mt5Account;

        public OrderController(MT5Account mt5Account)
        {
            _mt5Account = mt5Account;
        }

        [HttpGet("OrderSend")]
        public async Task<IActionResult> OrderSend()
        {
            var result = await _mt5Account.OrderSendAsync(new OrderSendRequest
            {
                Symbol = Constants.DefaultSymbol,
                Operation = (TMT5_ENUM_ORDER_TYPE)ENUM_ORDER_TYPE_TF.OrderTypeTfBuy,
                Volume = Constants.DefaultVolume,
                Price = 1.23456,
                Slippage = 10,
                Comment = "Test order"
            });

            return Ok(result);
        }

        [HttpGet("OrderModify")]
        public async Task<IActionResult> OrderModify()
        {
            var result = await _mt5Account.OrderModifyAsync(new OrderModifyRequest
            {
                Ticket = 12345678,
                Price = 1.23456,
                StopLoss = 1.23000,
                TakeProfit = 1.24000
            });

            return Ok(result);
        }

        [HttpGet("OrderClose")]
        public async Task<IActionResult> OrderClose()
        {
            var result = await _mt5Account.OrderCloseAsync(new OrderCloseRequest
            {
                Ticket = 12345678,
                Volume = Constants.DefaultVolume,               
                Slippage = 10
            });

            return Ok(result);
        }
    }
    
}
