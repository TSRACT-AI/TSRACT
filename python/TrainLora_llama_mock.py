import argparse
import time

parser = argparse.ArgumentParser()
parser.add_argument('--model_path', type=str, default='openlm-research/open_llama_7b')
parser.add_argument('--dataset_path', type=str, default='./data/demo.json')
parser.add_argument('--output_dir', type=str, default='./output/open_llama_7b_demo')
parser.add_argument('--bnb_load_in_4bit', type=bool, default=True)
parser.add_argument('--bnb_4bit_use_double_quant', type=bool, default=True)
parser.add_argument('--bnb_4bit_quant_type', type=str, default='nf4')
parser.add_argument('--lora_r', type=int, default=128)
parser.add_argument('--lora_alpha', type=int, default=256)
parser.add_argument('--lora_target_modules', type=str, default="q_proj,k_proj,v_proj")
parser.add_argument('--lora_dropout', type=float, default=0.05)
parser.add_argument('--lora_bias', type=str, default="none")
parser.add_argument('--lora_task_type', type=str, default="CAUSAL_LM")
parser.add_argument('--test_size', type=float, default=0.2)
parser.add_argument('--per_gpu_train_batch_size', type=int, default=1)
parser.add_argument('--gradient_accumulation_steps', type=int, default=4)
parser.add_argument('--warmup_ratio', type=float, default=0.1)
parser.add_argument('--num_train_epochs', type=int, default=3)
parser.add_argument('--learning_rate', type=float, default=2e-5)
parser.add_argument('--fp16', type=bool, default=True)
parser.add_argument('--save_steps', type=float, default=0.1)
parser.add_argument('--optimizer', type=str, default="paged_adamw_8bit")
parser.add_argument('--report_to', type=str, default="wandb")
args = parser.parse_args()

print("TSRACT: Loading base model...")

# wait 10 seconds
time.sleep(10)

print("TSRACT: Loading dataset...")

time.sleep(1)

print("TSRACT: Training...")

# loop for 10 minutes (count up in seconds with a time.sleep(1) every iteration)
for i in range(600):
	time.sleep(1)
	print(f"Test Status: {i}/600")

print("TSRACT: Finished")
